// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Windows.Media.Media3D;

/// <summary>Represents a Media3D cylinder.</summary>
public class Cylinder : ModelVisual3D
{
	private readonly GeometryModel3D model;
	private double radius;
	private int slices = 32;
	private double length = 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="Cylinder"/> class.
	/// </summary>
	public Cylinder()
	{
		this.model = new GeometryModel3D { Geometry = this.CalculateMesh() };
		this.Content = this.model;
	}

	/// <summary>Gets or sets the radius of the cylinder.</summary>
	public double Radius
	{
		get => this.radius;
		set
		{
			this.radius = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	/// <summary>
	/// Gets or sets the number of slices (segments) used to approximate the cylinder.
	/// </summary>
	public int Slices
	{
		get => this.slices;
		set
		{
			this.slices = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	/// <summary>Gets or sets the length of the cylinder.</summary>
	public double Length
	{
		get => this.length;
		set
		{
			this.length = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	/// <summary>Gets or sets the material applied to the cylinder.</summary>
	public Material Material
	{
		get => this.model.Material;
		set => this.model.Material = value;
	}

	/// <summary>
	/// Calculates the mesh geometry for the cylinder.
	/// </summary>
	/// <returns>A <see cref="MeshGeometry3D"/> representing the cylinder.</returns>
	private MeshGeometry3D CalculateMesh()
	{
		var mesh = new MeshGeometry3D();
		double radius = this.Radius;
		int slices = this.Slices;
		double length = this.Length;
		var axis = new Vector3D(0, length, 0);
		var endPoint = new Point3D(0, -(length / 2), 0);

		// Get two vectors perpendicular to the axis.
		Vector3D v1 = (axis.Z < -0.01 || axis.Z > 0.01)
			? new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y)
			: new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);

		Vector3D v2 = Vector3D.CrossProduct(v1, axis);

		// Make the vectors have length radius.
		v1 *= radius / v1.Length;
		v2 *= radius / v2.Length;

		// Pre-compute angles
		double dtheta = 2 * Math.PI / slices;
		double[] cosTheta = new double[slices];
		double[] sinTheta = new double[slices];
		for (int i = 0; i < slices; i++)
		{
			double theta = i * dtheta;
			cosTheta[i] = Math.Cos(theta);
			sinTheta[i] = Math.Sin(theta);
		}

		// Make the top end cap.
		int pt0 = mesh.Positions.Count;
		mesh.Positions.Add(endPoint);

		for (int i = 0; i < slices; i++)
		{
			mesh.Positions.Add(endPoint + (cosTheta[i] * v1) + (sinTheta[i] * v2));
		}

		int pt1 = mesh.Positions.Count - 1;
		int pt2 = pt0 + 1;
		for (int i = 0; i < slices; i++)
		{
			mesh.TriangleIndices.Add(pt0);
			mesh.TriangleIndices.Add(pt1);
			mesh.TriangleIndices.Add(pt2);
			pt1 = pt2++;
		}

		// Make the bottom end cap.
		pt0 = mesh.Positions.Count;
		Point3D endPoint2 = endPoint + axis;
		mesh.Positions.Add(endPoint2);

		for (int i = 0; i < slices; i++)
		{
			mesh.Positions.Add(endPoint2 + (cosTheta[i] * v1) + (sinTheta[i] * v2));
		}

		pt1 = mesh.Positions.Count - 1;
		pt2 = pt0 + 1;
		for (int i = 0; i < slices; i++)
		{
			mesh.TriangleIndices.Add(slices + 1);
			mesh.TriangleIndices.Add(pt2);
			mesh.TriangleIndices.Add(pt1);
			pt1 = pt2++;
		}

		// Make the sides.
		int firstSidePoint = mesh.Positions.Count;
		for (int i = 0; i < slices; i++)
		{
			Point3D p1 = endPoint + (cosTheta[i] * v1) + (sinTheta[i] * v2);
			mesh.Positions.Add(p1);
			mesh.Positions.Add(p1 + axis);
		}

		pt1 = mesh.Positions.Count - 2;
		pt2 = pt1 + 1;
		int pt3 = firstSidePoint;
		int pt4 = pt3 + 1;
		for (int i = 0; i < slices; i++)
		{
			mesh.TriangleIndices.Add(pt1);
			mesh.TriangleIndices.Add(pt2);
			mesh.TriangleIndices.Add(pt4);

			mesh.TriangleIndices.Add(pt1);
			mesh.TriangleIndices.Add(pt4);
			mesh.TriangleIndices.Add(pt3);

			pt1 = pt3;
			pt3 += 2;
			pt2 = pt4;
			pt4 += 2;
		}

		return mesh;
	}
}
