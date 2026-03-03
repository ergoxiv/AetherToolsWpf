// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Media3D;

/// <summary>
/// A manager that streamlines the rendering of <see cref="Line"/> objects in the viewport.
/// Without this manager, each <see cref="Line"/> object would need to calculate its
/// view-projection matrix, which adds significant overhead when many lines are present.
/// </summary>
/// <remarks>
/// The use of the manager is recommended over individual object rendering for performance reasons.
/// </remarks>
public sealed class LineManager
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="LineManager"/>.
	/// </summary>
	public static readonly Lazy<LineManager> Instance = new(() => new LineManager());

	private readonly Lock objLock = new();
	private readonly HashSet<Line> lines = [];
	private readonly Dictionary<Viewport3DVisual, Matrix4x4> viewportMatrixCache = [];

	private LineManager()
	{
		CompositionTarget.Rendering += this.OnRendering;
	}

	/// <summary>
	/// Registers a new <see cref="Line"/> object to the manager.
	/// </summary>
	/// <param name="line">
	/// The <see cref="Line"/> object to register.
	/// </param>
	public void Register(Line line)
	{
		lock (this.objLock)
		{
			this.lines.Add(line);
		}
	}

	/// <summary>
	/// Unregisters an existing <see cref="Line"/> object from the manager.
	/// </summary>
	/// <param name="line">
	/// The <see cref="Line"/> object to unregister.
	/// </param>
	public void Unregister(Line line)
	{
		lock (this.objLock)
		{
			this.lines.Remove(line);
		}
	}

	private void OnRendering(object? sender, EventArgs e)
	{
		this.viewportMatrixCache.Clear();

		lock (this.objLock)
		{
			if (this.lines.Count == 0)
				return;

			foreach (Line line in this.lines)
			{
				if (!line.IsVisible)
					continue;

				Viewport3DVisual? viewport = line.GetOrFindViewport();
				if (viewport == null)
					continue;

				if (!this.viewportMatrixCache.TryGetValue(viewport, out Matrix4x4 viewProjScreen))
				{
					if (MathUtils.TryGetViewProjectionViewportMatrix(viewport, out viewProjScreen))
						this.viewportMatrixCache[viewport] = viewProjScreen;
					else
						continue; // Invalid camera or viewport
				}

				line.UpdateGeometry(viewProjScreen);
			}
		}
	}
}
