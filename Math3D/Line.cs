// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XivToolsWpf.Math3D.Extensions;

/// <summary>Represents a Media3D line.</summary>
public class Line : ModelVisual3D, IDisposable
{
	private const float APPROX_EQUALITY_EPSILON = 1e-6f;

	/// <summary>Identifies the <see cref="Color"/> dependency property.</summary>
	public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(Line), new PropertyMetadata(Colors.White, OnColorChanged));

	/// <summary>Identifies the <see cref="Thickness"/> dependency property.</summary>
	public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(Line), new PropertyMetadata(1.0, OnThicknessChanged));

	/// <summary>Identifies the <see cref="Points"/> dependency property.</summary>
	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(nameof(Points), typeof(Point3DCollection), typeof(Line), new PropertyMetadata(null, OnPointsChanged));

	private readonly GeometryModel3D model;
	private readonly MeshGeometry3D mesh;

	private Matrix4x4 visualToScreen;
	private Matrix4x4 screenToVisual;
	private Viewport3DVisual? cachedViewport;
	private Visual3D? cachedRoot3D;

	/// <summary>
	/// Initializes a new instance of the <see cref="Line"/> class.
	/// </summary>
	public Line()
	{
		this.mesh = new MeshGeometry3D();
		this.model = new GeometryModel3D { Geometry = this.mesh };
		this.SetColor(this.Color);

		this.Content = this.model;
		this.Points = [];

		LineManager.Instance.Value.Register(this);
	}

	/// <summary>Gets a value indicating whether the element is currently visible.</summary>
	public bool IsVisible { get; private set; } = true;

	/// <summary>Gets or sets the color of the line.</summary>
	public Color Color
	{
		get => (Color)this.GetValue(ColorProperty);
		set => this.SetValue(ColorProperty, value);
	}

	/// <summary>Gets or sets the thickness of the line.</summary>
	public double Thickness
	{
		get => (double)this.GetValue(ThicknessProperty);
		set => this.SetValue(ThicknessProperty, value);
	}

	/// <summary>Gets or sets the collection of points that define the line.</summary>
	public Point3DCollection Points
	{
		get => (Point3DCollection)this.GetValue(PointsProperty);
		set => this.SetValue(PointsProperty, value);
	}

	/// <summary>Releases all resources used by the <see cref="Line"/> class.</summary>
	public void Dispose()
	{
		LineManager.Instance.Value.Unregister(this);

		this.Points.Clear();
		this.Children.Clear();
		this.Content = null;
		this.cachedViewport = null;
		this.cachedRoot3D = null;

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Creates a wireframe representation of the specified 3D model.
	/// </summary>
	/// <param name="model">The 3D model to create a wireframe for.</param>
	public void MakeWireframe(Model3D model)
	{
		this.Points.Clear();

		if (model == null)
		{
			return;
		}

		var transform = new Matrix3DStack();
		transform.Push(Matrix3D.Identity);

		this.WireframeHelper(model, transform);
	}

	/// <summary>
	/// Finds the nearest point on the line to the specified camera point in 2D space.
	/// </summary>
	/// <param name="cameraPoint">The camera point to find the nearest point to.</param>
	/// <returns>The nearest point on the line, or null if no point is found.</returns>
	public Point3D? NearestPoint2D(Point3D cameraPoint)
	{
		if (this.Points.Count == 0 && this.mesh.Positions.Count == 0)
			return null;

		var viewport = this.GetOrFindViewport();
		if (viewport == null)
			return null;

		Matrix4x4? modelToWorld = this.TryGetModelToWorldMatrix();
		if (modelToWorld == null)
			return null;

		if (!MathUtils.TryTransformVisualToViewport(viewport, (Matrix4x4)modelToWorld, out Matrix4x4 matrix))
			return null;

		float closest = float.MaxValue;
		Point3D? closestPoint = null;

		foreach (Point3D point in this.Points)
		{
			Vector4 transformed4 = Vector4.Transform(point.FromMedia3DPoint(), matrix);
			var transformed = new Vector3(
				transformed4.X / transformed4.W,
				transformed4.Y / transformed4.W,
				transformed4.Z / transformed4.W);

			transformed.Z *= 100f;

			float dirLength = Vector3.Distance(cameraPoint.FromMedia3DPoint(), transformed);
			if (dirLength < closest)
			{
				closest = dirLength;
				closestPoint = point;
			}
		}

		return closestPoint;
	}

	/// <summary>
	/// Gets or finds the viewport that contains this line.
	/// </summary>
	/// <returns>
	/// The viewport that contains this line, or null if none is found.
	/// </returns>
	/// <remarks>
	/// If available, this method returns a cached viewport reference.
	/// </remarks>
	public Viewport3DVisual? GetOrFindViewport()
	{
		if (this.cachedViewport != null)
			return this.cachedViewport;

		this.cachedViewport = MathUtils.FindViewport(this, out this.cachedRoot3D);
		return this.cachedViewport;
	}

	/// <summary>
	/// Updates the transforms for the line.
	/// </summary>
	/// <param name="viewProjScreen">
	/// The view-projection-screen matrix.
	/// </param>
	/// <remarks>
	/// This method is intended to be called by the <see cref="LineManager"/> during rendering.
	/// </remarks>
	public void UpdateGeometry(in Matrix4x4 viewProjScreen)
	{
		if (this.Points.Count == 0 && this.mesh.Positions.Count == 0 || this.cachedRoot3D == null)
			return;

		Matrix4x4? modelToWorld = this.TryGetModelToWorldMatrix();
		if (modelToWorld == null)
			return;

		Matrix4x4 newV2S = (Matrix4x4)modelToWorld * viewProjScreen;
		if (newV2S.IsApproximately(this.visualToScreen, APPROX_EQUALITY_EPSILON))
			return;

		if (!Matrix4x4.Invert(newV2S, out Matrix4x4 newS2V))
			return;

		this.visualToScreen = newV2S;
		this.screenToVisual = newS2V;

		this.RebuildGeometry();
	}

	/// <inheritdoc/>
	protected override void OnVisualParentChanged(DependencyObject? oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		this.GeometryDirty();

		this.IsVisible = VisualTreeHelper.GetParent(this) != null;
	}

	/// <summary>
	/// Handles changes to the <see cref="Color"/> property.
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="args">The event data.</param>
	private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		((Line)sender).SetColor((Color)args.NewValue);
	}

	/// <summary>
	/// Handles changes to the <see cref="Thickness"/> property.
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="args">The event data.</param>
	private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		((Line)sender).GeometryDirty();
	}

	/// <summary>
	/// Handles changes to the <see cref="Points"/> property.
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="args">The event data.</param>
	private static void OnPointsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		((Line)sender).GeometryDirty();
	}

	/// <summary>
	/// Widens a point in 4D space by a given delta in screen space.
	/// </summary>
	/// <param name="pIn4">
	/// The input point in 4D space.
	/// </param>
	/// <param name="delta">
	/// The delta to apply in screen space.
	/// </param>
	/// <param name="s2v">
	/// The screen-to-visual transformation matrix.
	/// </param>
	/// <result>
	/// The widened point in 3D space.
	/// </result>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Point3D Widen(Vector4 pIn4, Vector2 delta, in Matrix4x4 s2v)
	{
		// Apply delta scaled by W
		pIn4.X += delta.X * pIn4.W;
		pIn4.Y += delta.Y * pIn4.W;

		// Un-project back to visual space
		Vector4 pOut4 = Vector4.Transform(pIn4, s2v);
		return new Point3D(pOut4.X / pOut4.W, pOut4.Y / pOut4.W, pOut4.Z / pOut4.W);
	}

	/// <summary>
	/// Sets the color of the line.
	/// </summary>
	/// <param name="color">The color to set.</param>
	private void SetColor(Color color)
	{
		var unlitMaterial = new MaterialGroup();
		unlitMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
		unlitMaterial.Children.Add(new EmissiveMaterial(new SolidColorBrush(color)));
		unlitMaterial.Freeze();

		this.model.Material = unlitMaterial;
		this.model.BackMaterial = unlitMaterial;
	}

	/// <summary>
	/// Marks the geometry as dirty, forcing a rebuild on the next render.
	/// </summary>
	private void GeometryDirty()
	{
		// Force next call to UpdateTransforms() to return true.
		this.cachedViewport = null;
		this.cachedRoot3D = null;
		this.visualToScreen = MathUtils.ZeroMatrix4x4;
	}

	/// <summary>Rebuilds the geometry of the line.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void RebuildGeometry()
	{
		var points = this.Points;
		int pointCount = points.Count;
		if (pointCount < 2)
			return;

		int numLines = pointCount / 2;
		int numVertices = numLines * 4;
		int numIndices = numLines * 6;

		Point3D[] vertexBuffer = ArrayPool<Point3D>.Shared.Rent(numVertices);
		var indices = new Int32Collection(numIndices);

		float halfThickness = (float)this.Thickness / 2.0f;
		Matrix4x4 v2s = this.visualToScreen;
		Matrix4x4 s2v = this.screenToVisual;

		try
		{
			for (int i = 0; i < numLines; i++)
			{
				int ptIdx = i * 2;
				int vBase = i * 4;

				Vector3 startP = points[ptIdx].FromMedia3DPoint();
				Vector3 endP = points[ptIdx + 1].FromMedia3DPoint();

				// Transform to homogeneous clip space
				Vector4 startClip = Vector4.Transform(startP, v2s);
				Vector4 endClip = Vector4.Transform(endP, v2s);

				if (startClip.W <= 0f || endClip.W <= 0f)
					continue;

				// Compute screen-space direction
				Vector2 sStart = new(startClip.X / startClip.W, startClip.Y / startClip.W);
				Vector2 sEnd = new(endClip.X / endClip.W, endClip.Y / endClip.W);

				Vector2 lineDir = sEnd - sStart;
				float len = lineDir.Length();

				Vector2 delta;
				if (len < APPROX_EQUALITY_EPSILON)
				{
					delta = new Vector2(halfThickness, 0);
				}
				else
				{
					// Perpendicular vector in screen space
					delta = new Vector2(-lineDir.Y, lineDir.X) * (halfThickness / len);
				}

				// Widen and invert
				// We scale the delta by W to keep thickness constant in screen space
				vertexBuffer[vBase + 0] = Widen(startClip, delta, s2v);
				vertexBuffer[vBase + 1] = Widen(startClip, -delta, s2v);
				vertexBuffer[vBase + 2] = Widen(endClip, delta, s2v);
				vertexBuffer[vBase + 3] = Widen(endClip, -delta, s2v);

				// Indexing
				indices.Add(vBase + 2);
				indices.Add(vBase + 1);
				indices.Add(vBase + 0);

				indices.Add(vBase + 2);
				indices.Add(vBase + 3);
				indices.Add(vBase + 1);
			}

			this.mesh.Positions = [.. vertexBuffer.AsSpan(0, numVertices).ToArray()];
			this.mesh.Positions.Freeze();
			this.mesh.TriangleIndices = indices;
			this.mesh.TriangleIndices.Freeze();
		}
		finally
		{
			ArrayPool<Point3D>.Shared.Return(vertexBuffer);
		}
	}

	/// <summary>Helper method to create a wireframe representation of a 3D model.</summary>
	/// <param name="model">The 3D model to create a wireframe for.</param>
	/// <param name="matrixStack">The matrix stack to use for transformations.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WireframeHelper(Model3D model, Matrix3DStack matrixStack)
	{
		Transform3D transform = model.Transform;

		if (transform != null && transform != Transform3D.Identity)
		{
			matrixStack.Prepend(model.Transform.Value);
		}

		try
		{
			if (model is Model3DGroup group)
			{
				this.WireframeHelper(group, matrixStack);
				return;
			}

			if (model is GeometryModel3D geometry)
			{
				this.WireframeHelper(geometry, matrixStack);
				return;
			}
		}
		finally
		{
			if (transform != null && transform != Transform3D.Identity)
			{
				matrixStack.Pop();
			}
		}
	}

	/// <summary>
	/// Helper method to create a wireframe representation of a 3D model group.
	/// </summary>
	/// <param name="group">The 3D model group to create a wireframe for.</param>
	/// <param name="matrixStack">The matrix stack to use for transformations.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WireframeHelper(Model3DGroup group, Matrix3DStack matrixStack)
	{
		foreach (Model3D child in group.Children)
		{
			this.WireframeHelper(child, matrixStack);
		}
	}

	/// <summary>
	/// Helper method to create a wireframe representation of a geometry model.
	/// </summary>
	/// <param name="model">The geometry model to create a wireframe for.</param>
	/// <param name="matrixStack">The matrix stack to use for transformations.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WireframeHelper(GeometryModel3D model, Matrix3DStack matrixStack)
	{
		Geometry3D geometry = model.Geometry;

		if (geometry is MeshGeometry3D mesh)
		{
			Point3D[] positions = new Point3D[mesh.Positions.Count];
			mesh.Positions.CopyTo(positions, 0);
			matrixStack.Peek().Transform(positions);

			Int32Collection indices = mesh.TriangleIndices;

			if (indices.Count > 0)
			{
				int limit = positions.Length - 1;

				for (int i = 2, count = indices.Count; i < count; i += 3)
				{
					int i0 = indices[i - 2];
					int i1 = indices[i - 1];
					int i2 = indices[i];

					// WPF halts rendering on the first deformed triangle. We should do the same.
					if ((i0 < 0 || i0 > limit) || (i1 < 0 || i1 > limit) || (i2 < 0 || i2 > limit))
					{
						break;
					}

					this.AddTriangle(ref positions, i0, i1, i2);
				}
			}
			else
			{
				for (int i = 2, count = positions.Length; i < count; i += 3)
				{
					int i0 = i - 2;
					int i1 = i - 1;
					int i2 = i;

					this.AddTriangle(ref positions, i0, i1, i2);
				}
			}
		}
	}

	/// <summary>
	/// Adds a triangle to the line geometry.
	/// </summary>
	/// <param name="positions">The array of positions.</param>
	/// <param name="i0">The first index of the triangle.</param>
	/// <param name="i1">The second index of the triangle.</param>
	/// <param name="i2">The third index of the triangle.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddTriangle(ref Point3D[] positions, int i0, int i1, int i2)
	{
		this.Points.Add(positions[i0]);
		this.Points.Add(positions[i1]);
		this.Points.Add(positions[i1]);
		this.Points.Add(positions[i2]);
		this.Points.Add(positions[i2]);
		this.Points.Add(positions[i0]);
	}

	private Matrix4x4? TryGetModelToWorldMatrix()
	{
		if (this.cachedRoot3D == null)
			return null;

		try
		{
			GeneralTransform3D transform = this.TransformToAncestor(this.cachedRoot3D);

			Matrix4x4 modelToWorld;
			if (transform is Transform3D t3d)
			{
				modelToWorld = t3d.Value.ToMatrix4x4();
			}
			else
			{
				modelToWorld = Matrix4x4.Identity;
			}

			if (this.cachedRoot3D.Transform != null && !this.cachedRoot3D.Transform.Value.IsIdentity)
			{
				modelToWorld *= this.cachedRoot3D.Transform.Value.ToMatrix4x4();
			}

			return modelToWorld;
		}
		catch (InvalidOperationException)
		{
			this.cachedViewport = null;
			return null;
		}
	}
}
