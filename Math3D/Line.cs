// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

/// <summary>Represents a Media3D line.</summary>
public class Line : ModelVisual3D, IDisposable
{
	/// <summary>Identifies the <see cref="Color"/> dependency property.</summary>
	public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(Line), new PropertyMetadata(Colors.White, OnColorChanged));

	/// <summary>Identifies the <see cref="Thickness"/> dependency property.</summary>
	public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(Line), new PropertyMetadata(1.0, OnThicknessChanged));

	/// <summary>Identifies the <see cref="Points"/> dependency property.</summary>
	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(nameof(Points), typeof(Point3DCollection), typeof(Line), new PropertyMetadata(null, OnPointsChanged));

	private readonly GeometryModel3D model;
	private readonly MeshGeometry3D mesh;

	private Matrix3D visualToScreen;
	private Matrix3D screenToVisual;

	/// <summary>
	/// Initializes a new instance of the <see cref="Line"/> class.
	/// </summary>
	public Line()
	{
		this.mesh = new MeshGeometry3D();
		this.model = new GeometryModel3D { Geometry = this.mesh };
		this.SetColor(this.Color);

		this.Content = this.model;
		this.Points = new Point3DCollection();

		CompositionTarget.Rendering += this.OnRender;
	}

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
		CompositionTarget.Rendering -= this.OnRender;

		this.Points.Clear();
		this.Children.Clear();
		this.Content = null;

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
		double closest = double.MaxValue;
		Point3D? closestPoint = null;

		if (!MathUtils.ToViewportTransform(this, out Matrix3D matrix))
			return null;

		var transform = new MatrixTransform3D(matrix);

		foreach (Point3D point in this.Points)
		{
			Point3D cameraSpacePoint = transform.Transform(point);
			cameraSpacePoint.Z *= 100;

			Vector3D dir = cameraPoint - cameraSpacePoint;
			if (dir.Length < closest)
			{
				closest = dir.Length;
				closestPoint = point;
			}
		}

		return closestPoint;
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
	/// Handles the rendering event to update the line geometry.
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="e">The event data.</param>
	private void OnRender(object? sender, EventArgs e)
	{
		if (this.Points.Count == 0 && this.mesh.Positions.Count == 0)
			return;

		if (this.UpdateTransforms())
		{
			this.RebuildGeometry();
		}
	}

	/// <summary>
	/// Marks the geometry as dirty, forcing a rebuild on the next render.
	/// </summary>
	private void GeometryDirty()
	{
		// Force next call to UpdateTransforms() to return true.
		this.visualToScreen = MathUtils.ZeroMatrix;
	}

	/// <summary>Rebuilds the geometry of the line.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void RebuildGeometry()
	{
		double halfThickness = this.Thickness / 2.0;
		int numLines = this.Points.Count / 2;

		var positions = new Point3DCollection(numLines * 4);

		for (int i = 0; i < numLines; i++)
		{
			int startIndex = i * 2;

			Point3D startPoint = this.Points[startIndex];
			Point3D endPoint = this.Points[startIndex + 1];

			this.AddSegment(positions, startPoint, endPoint, halfThickness);
		}

		positions.Freeze();
		this.mesh.Positions = positions;

		var indices = new Int32Collection(this.Points.Count * 3);

		for (int i = 0; i < this.Points.Count / 2; i++)
		{
			indices.Add((i * 4) + 2);
			indices.Add((i * 4) + 1);
			indices.Add((i * 4) + 0);

			indices.Add((i * 4) + 2);
			indices.Add((i * 4) + 3);
			indices.Add((i * 4) + 1);
		}

		indices.Freeze();
		this.mesh.TriangleIndices = indices;
	}

	/// <summary>Adds a segment to the line geometry.</summary>
	/// <param name="positions">The collection of positions to add to.</param>
	/// <param name="startPoint">The start point of the segment.</param>
	/// <param name="endPoint">The end point of the segment.</param>
	/// <param name="halfThickness">Half the thickness of the line.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddSegment(Point3DCollection positions, Point3D startPoint, Point3D endPoint, double halfThickness)
	{
		// NOTE: We want the vector below to be perpendicular post projection so
		//       we need to compute the line direction in post-projective space.
		Vector3D lineDirection = (endPoint * this.visualToScreen) - (startPoint * this.visualToScreen);
		lineDirection.Z = 0;
		lineDirection.Normalize();

		// NOTE: Implicit Rot(90) during construction to get a perpendicular vector.
		var delta = new Vector(-lineDirection.Y, lineDirection.X);
		delta *= halfThickness;

		this.Widen(startPoint, delta, out Point3D pOut1, out Point3D pOut2);

		positions.Add(pOut1);
		positions.Add(pOut2);

		this.Widen(endPoint, delta, out pOut1, out pOut2);

		positions.Add(pOut1);
		positions.Add(pOut2);
	}

	/// <summary>Widens a point by a specified delta.</summary>
	/// <param name="pIn">The input point.</param>
	/// <param name="delta">The delta to widen by.</param>
	/// <param name="pOut1">The first widened point.</param>
	/// <param name="pOut2">The second widened point.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Widen(Point3D pIn, Vector delta, out Point3D pOut1, out Point3D pOut2)
	{
		Point4D pIn4 = (Point4D)pIn;
		Point4D pOut41 = pIn4 * this.visualToScreen;
		Point4D pOut42 = pOut41;

		pOut41.X += delta.X * pOut41.W;
		pOut41.Y += delta.Y * pOut41.W;

		pOut42.X -= delta.X * pOut42.W;
		pOut42.Y -= delta.Y * pOut42.W;

		pOut41 *= this.screenToVisual;
		pOut42 *= this.screenToVisual;

		// NOTE: Z is not modified above, so we use the original Z below.
		pOut1 = new Point3D(pOut41.X / pOut41.W, pOut41.Y / pOut41.W, pOut41.Z / pOut41.W);
		pOut2 = new Point3D(pOut42.X / pOut42.W, pOut42.Y / pOut42.W, pOut42.Z / pOut42.W);
	}

	/// <summary>Updates the transforms for the line.</summary>
	/// <returns>true if the transforms were updated; otherwise, false.</returns>
	private bool UpdateTransforms()
	{
		Matrix3D visualToScreen = MathUtils.TryTransformTo2DAncestor(this, out Viewport3DVisual? viewport, out bool success);

		if (!success || !visualToScreen.HasInverse)
		{
			this.mesh.Positions = null;
			return false;
		}

		if (visualToScreen == this.visualToScreen)
		{
			return false;
		}

		this.visualToScreen = this.screenToVisual = visualToScreen;
		this.screenToVisual.Invert();

		return true;
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

					this.AddTriangle(positions, i0, i1, i2);
				}
			}
			else
			{
				for (int i = 2, count = positions.Length; i < count; i += 3)
				{
					int i0 = i - 2;
					int i1 = i - 1;
					int i2 = i;

					this.AddTriangle(positions, i0, i1, i2);
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
	private void AddTriangle(Point3D[] positions, int i0, int i1, int i2)
	{
		this.Points.Add(positions[i0]);
		this.Points.Add(positions[i1]);
		this.Points.Add(positions[i1]);
		this.Points.Add(positions[i2]);
		this.Points.Add(positions[i2]);
		this.Points.Add(positions[i0]);
	}
}
