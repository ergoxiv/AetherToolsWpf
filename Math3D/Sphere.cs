// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

public class Sphere : ModelVisual3D
{
	private readonly GeometryModel3D model;
	private int slices = 32;
	private int stacks = 16;
	private double radius = 1;
	private Point3D center = default;

	public Sphere()
	{
		this.model = new GeometryModel3D();
		this.model.Geometry = this.CalculateMesh();
		this.Content = this.model;
	}

	public int Slices
	{
		get
		{
			return this.slices;
		}
		set
		{
			this.slices = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	public int Stacks
	{
		get
		{
			return this.stacks;
		}
		set
		{
			this.stacks = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	public double Radius
	{
		get
		{
			return this.radius;
		}
		set
		{
			this.radius = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	public Material Material
	{
		get
		{
			return this.model.Material;
		}

		set
		{
			this.model.Material = value;
		}
	}

	private MeshGeometry3D CalculateMesh()
	{
		MeshGeometry3D mesh = new();

		int slices = this.Slices;
		int stacks = this.Stacks;

		int totalVertices = (stacks + 1) * (slices + 1);
		int totalTriangles = stacks * slices * 6;

		// Pre-allocation to avoid resizing
		mesh.Positions = new Point3DCollection(totalVertices);
		mesh.Normals = new Vector3DCollection(totalVertices);
		mesh.TextureCoordinates = new PointCollection(totalVertices);
		mesh.TriangleIndices = new Int32Collection(totalTriangles);

		double radius = this.Radius;
		Point3D center = this.center;

		for (int stack = 0; stack <= stacks; stack++)
		{
			double phi = (Math.PI / 2) - (stack * Math.PI / stacks);
			double y = radius * Math.Sin(phi);
			double scale = -radius * Math.Cos(phi);

			for (int slice = 0; slice <= slices; slice++)
			{
				double theta = slice * 2 * Math.PI / slices;
				double x = scale * Math.Sin(theta);
				double z = scale * Math.Cos(theta);

				Vector3D normal = new(x, y, z);
				mesh.Normals.Add(normal);
				mesh.Positions.Add(center + normal);
				mesh.TextureCoordinates.Add(new Point((double)slice / slices, (double)stack / stacks));
			}
		}

		for (int stack = 0; stack < stacks; stack++)
		{
			int top = stack * (slices + 1);
			int bot = (stack + 1) * (slices + 1);

			for (int slice = 0; slice < slices; slice++)
			{
				if (stack != 0)
				{
					mesh.TriangleIndices.Add(top + slice);
					mesh.TriangleIndices.Add(bot + slice);
					mesh.TriangleIndices.Add(top + slice + 1);
				}

				if (stack != stacks - 1)
				{
					mesh.TriangleIndices.Add(top + slice + 1);
					mesh.TriangleIndices.Add(bot + slice);
					mesh.TriangleIndices.Add(bot + slice + 1);
				}
			}
		}

		return mesh;
	}
}
