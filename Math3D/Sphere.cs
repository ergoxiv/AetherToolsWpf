// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

/// <summary>Represents a Media3D sphere.</summary>
public class Sphere : ModelVisual3D, IDisposable
{
	private const int DEFAULT_SLICES = 32;
	private const int DEFAULT_STACKS = 16;
	private const int DEFAULT_RADIUS = 1;

	private static readonly Dictionary<(int slices, int stacks, double radius), MeshGeometry3D> MeshCache = [];

	private readonly GeometryModel3D model;
	private int slices = DEFAULT_SLICES;
	private int stacks = DEFAULT_STACKS;
	private double radius = DEFAULT_RADIUS;
	private bool disposed = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sphere"/> class.
	/// </summary>
	/// <param name="cacheMesh">
	/// Indicates whether to use the mesh cache for the new instance.
	/// The default mode is <c>true</c> to reduce memory allocations.
	/// </param>
	public Sphere(bool cacheMesh = true)
		: this(DEFAULT_SLICES, DEFAULT_STACKS, DEFAULT_RADIUS, cacheMesh)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Sphere"/> class.
	/// </summary>
	/// <param name="radius">The radius of the sphere.</param>
	/// <param name="cacheMesh">
	/// Indicates whether to use the mesh cache for the new instance.
	/// The default mode is <c>true</c> to reduce memory allocations.
	/// </param>
	public Sphere(double radius, bool cacheMesh = true)
		: this(DEFAULT_SLICES, DEFAULT_STACKS, radius, cacheMesh)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Sphere"/> class.
	/// </summary>
	/// <param name="slices">The number of vertical divisions.</param>
	/// <param name="stacks">The number of horizontal divisions.</param>
	/// <param name="radius">The radius of the sphere.</param>
	/// <param name="cacheMesh">
	/// Indicates whether to use the mesh cache for the new instance.
	/// The default mode is <c>true</c> to reduce memory allocations.
	/// </param>
	public Sphere(int slices, int stacks, double radius, bool cacheMesh = true)
	{
		this.slices = slices;
		this.stacks = stacks;
		this.radius = radius;
		this.UseMeshCache = cacheMesh;
		this.model = new GeometryModel3D { Geometry = this.GetOrCreateMesh(this.slices, this.stacks, this.radius) };
		this.Content = this.model;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Sphere"/> class.
	/// </summary>
	/// <param name="other">The sphere to copy from.</param>
	/// <param name="cacheMesh">
	/// Indicates whether to use the mesh cache for the new instance.
	/// The default mode is <c>true</c> to reduce memory allocations.
	/// </param>
	public Sphere(Sphere other, bool cacheMesh = true)
	{
		this.UseMeshCache = cacheMesh;

		if (cacheMesh)
		{
			this.model = new GeometryModel3D
			{
				Geometry = this.GetOrCreateMesh(other.slices, other.stacks, other.radius),
				Material = other.model.Material,
			};
		}
		else
		{
			this.model = new GeometryModel3D
			{
				Geometry = CloneMeshGeometry3D(other.model.Geometry as MeshGeometry3D),
				Material = other.model.Material,
			};
		}

		this.Content = this.model;
		this.slices = other.slices;
		this.stacks = other.stacks;
		this.radius = other.radius;
	}

	/// <summary>
	/// Gets a value indicating whether the sphere uses a shared mesh cache.
	/// </summary>
	/// <remarks>
	/// This value can only be set at construction time and cannot be changed afterwards.
	/// This is to prevent issues with mesh data disposal.
	/// </remarks>
	public bool UseMeshCache { get; private set; }

	/// <summary>
	/// Gets or sets the number of slices (vertical divisions) of the sphere.
	/// </summary>
	public int Slices
	{
		get => this.slices;
		set
		{
			this.slices = value;
			this.model.Geometry = this.GetOrCreateMesh(this.slices, this.stacks, this.radius);
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
			this.model.Geometry = this.GetOrCreateMesh(this.slices, this.stacks, this.radius);
		}
	}

	/// <summary>Gets or sets the radius of the sphere.</summary>
	public double Radius
	{
		get => this.radius;
		set
		{
			this.radius = value;
			this.model.Geometry = this.GetOrCreateMesh(this.slices, this.stacks, this.radius);
		}
	}

	/// <summary>Gets or sets the material of the sphere.</summary>
	public Material Material
	{
		get => this.model.Material;
		set => this.model.Material = value;
	}

	/// <summary>
	/// Clears the cached mesh geometries.
	/// </summary>
	/// <remarks>
	/// Existing <see cref="Sphere"/> instances will be unaffected, but new instances
	/// will need to recalculate their meshes.
	/// </remarks>
	public static void ClearMeshCache() => MeshCache.Clear();

	/// <summary>
	/// Disposes the resources used by the <see cref="Sphere"/> class.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the resources used by the <see cref="Sphere"/> class.
	/// </summary>
	/// <param name="disposing">Indicates whether the method is called from Dispose or the finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing && !this.UseMeshCache)
			{
				if (this.model.Geometry is MeshGeometry3D mesh)
				{
					mesh.Positions.Clear();
					mesh.Normals.Clear();
					mesh.TextureCoordinates.Clear();
					mesh.TriangleIndices.Clear();
				}
			}

			this.disposed = true;
		}
	}

	/// <summary>
	/// Clones a <see cref="MeshGeometry3D"/> instance.
	/// </summary>
	/// <param name="mesh">The mesh to clone.</param>
	/// <returns>A new <see cref="MeshGeometry3D"/> instance with the same data.</returns>
	private static MeshGeometry3D CloneMeshGeometry3D(MeshGeometry3D? mesh)
	{
		ArgumentNullException.ThrowIfNull(mesh);

		return new MeshGeometry3D
		{
			Positions = new Point3DCollection(mesh.Positions),
			Normals = new Vector3DCollection(mesh.Normals),
			TextureCoordinates = new PointCollection(mesh.TextureCoordinates),
			TriangleIndices = new Int32Collection(mesh.TriangleIndices),
		};
	}

	/// <summary>
	/// Calculates the mesh geometry for the sphere based on the current properties.
	/// </summary>
	/// <returns>A <see cref="MeshGeometry3D"/> representing the sphere.</returns>
	private static MeshGeometry3D CalculateMesh(int slices, int stacks, double radius)
	{
		var mesh = new MeshGeometry3D();
		Point3D center = default;

		// Pre-allocate the collections to the correct size.
		int totalVertices = (stacks + 1) * (slices + 1);
		mesh.Positions = new Point3DCollection(totalVertices);
		mesh.Normals = new Vector3DCollection(totalVertices);
		mesh.TextureCoordinates = new PointCollection(totalVertices);

		// Calculate the step size for phi (latitude) and theta (longitude)
		// Micro-optimization: Calcualted once instead of every iteration.
		double phiStep = Math.PI / stacks;
		double thetaStep = 2 * Math.PI / slices;

		// Generate the vertices, normals, and texture coordinates
		for (int stack = 0; stack <= stacks; stack++)
		{
			double phi = (Math.PI / 2) - (stack * phiStep); // Latitude angle
			double y = radius * Math.Sin(phi);              // Y-coord
			double scale = -radius * Math.Cos(phi);         // Radius at current latitude

			for (int slice = 0; slice <= slices; slice++)
			{
				double theta = slice * thetaStep;           // Longitude angle
				double x = scale * Math.Sin(theta);         // X-coord
				double z = scale * Math.Cos(theta);         // Z-coord

				var normal = new Vector3D(x, y, z);
				mesh.Normals.Add(normal);
				mesh.Positions.Add(center + normal);
				mesh.TextureCoordinates.Add(new Point((double)slice / slices, (double)stack / stacks));
			}
		}

		// Pre-allocate the collection to the correct size.
		int totalIndices = stacks * slices * 6;
		mesh.TriangleIndices = new Int32Collection(totalIndices);

		// Generate the indices for the triangles
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

	/// <summary>
	/// Gets or creates a cached mesh geometry for the sphere based on the specified parameters.
	/// </summary>
	/// <param name="slices">The number of vertical divisions.</param>
	/// <param name="stacks">The number of horizontal divisions.</param>
	/// <param name="radius">The radius of the sphere.</param>
	/// <returns>The cached or newly created <see cref="MeshGeometry3D"/>.</returns>
	private MeshGeometry3D GetOrCreateMesh(int slices, int stacks, double radius)
	{
		if (this.UseMeshCache)
		{
			var key = (slices, stacks, radius);
			if (!MeshCache.TryGetValue(key, out var mesh))
			{
				mesh = CalculateMesh(slices, stacks, radius);
				MeshCache[key] = mesh;
			}
			return mesh;
		}
		else
		{
			return CalculateMesh(slices, stacks, radius);
		}
	}
}
