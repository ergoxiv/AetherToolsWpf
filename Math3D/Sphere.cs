// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

/// <summary>Represents a Media3D sphere.</summary>
public class Sphere : ModelVisual3D
{
	private readonly GeometryModel3D model;
	private int slices = 32;
	private int stacks = 16;
	private double radius = 1;
	private Point3D center = default;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sphere"/> class.
	/// </summary>
	public Sphere()
	{
		this.model = new GeometryModel3D { Geometry = this.CalculateMesh() };
		this.Content = this.model;
	}

	/// <summary>
	/// Gets or sets the number of slices (vertical divisions) of the sphere.
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

	/// <summary>
	/// Gets or sets the number of stacks (horizontal divisions) of the sphere.
	/// </summary>
	public int Stacks
	{
		get => this.stacks;
		set
		{
			this.stacks = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	/// <summary>Gets or sets the radius of the sphere.</summary>
	public double Radius
	{
		get => this.radius;
		set
		{
			this.radius = value;
			this.model.Geometry = this.CalculateMesh();
		}
	}

	/// <summary>Gets or sets the material of the sphere.</summary>
	public Material Material
	{
		get => this.model.Material;
		set => this.model.Material = value;
	}

	/// <summary>
	/// Calculates the mesh geometry for the sphere based on the current properties.
	/// </summary>
	/// <returns>A <see cref="MeshGeometry3D"/> representing the sphere.</returns>
	private MeshGeometry3D CalculateMesh()
	{
		var mesh = new MeshGeometry3D();

		// Pre-allocate the collections to the correct size.
		int totalVertices = (this.stacks + 1) * (this.slices + 1);
		mesh.Positions = new Point3DCollection(totalVertices);
		mesh.Normals = new Vector3DCollection(totalVertices);
		mesh.TextureCoordinates = new PointCollection(totalVertices);

		// Calculate the step size for phi (latitude) and theta (longitude)
		// Micro-optimization: Calcualted once instead of every iteration.
		double phiStep = Math.PI / this.stacks;
		double thetaStep = 2 * Math.PI / this.slices;

		// Generate the vertices, normals, and texture coordinates
		for (int stack = 0; stack <= this.stacks; stack++)
		{
			double phi = (Math.PI / 2) - (stack * phiStep); // Latitude angle
			double y = this.radius * Math.Sin(phi);         // Y-coord
			double scale = -this.radius * Math.Cos(phi);    // Radius at current latitude

			for (int slice = 0; slice <= this.slices; slice++)
			{
				double theta = slice * thetaStep;           // Longitude angle
				double x = scale * Math.Sin(theta);         // X-coord
				double z = scale * Math.Cos(theta);         // Z-coord

				var normal = new Vector3D(x, y, z);
				mesh.Normals.Add(normal);
				mesh.Positions.Add(this.center + normal);
				mesh.TextureCoordinates.Add(new Point((double)slice / this.slices, (double)stack / this.stacks));
			}
		}

		// Pre-allocate the collection to the correct size.
		int totalIndices = this.stacks * this.slices * 6;
		mesh.TriangleIndices = new Int32Collection(totalIndices);

		// Generate the indices for the triangles
		for (int stack = 0; stack < this.stacks; stack++)
		{
			int top = stack * (this.slices + 1);
			int bot = (stack + 1) * (this.slices + 1);

			for (int slice = 0; slice < this.slices; slice++)
			{
				if (stack != 0)
				{
					mesh.TriangleIndices.Add(top + slice);
					mesh.TriangleIndices.Add(bot + slice);
					mesh.TriangleIndices.Add(top + slice + 1);
				}

				if (stack != this.stacks - 1)
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
