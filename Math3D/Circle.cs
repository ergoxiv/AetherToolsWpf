// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System;
using System.Windows.Media.Media3D;

/// <summary>Represents a Media3D circle.</summary>
public class Circle : Line
{
	private const int Segments = 360;
	private double radius;

	/// <summary>
	/// Initializes a new instance of the <see cref="Circle"/> class.
	/// </summary>
	public Circle()
	{
		this.Generate();
	}

	/// <summary>Gets or sets the radius of the circle.</summary>
	public double Radius
	{
		get => this.radius;
		set
		{
			this.radius = value;
			this.Generate();
		}
	}

	/// <summary>Generates the points that make up the circle.</summary>
	public void Generate()
	{
		this.Points.Clear();

		double angleStep = MathUtils.DegreesToRadians(360.0 / Segments);
		double angle = 0.0;

		for (int i = 0; i < Segments; i++)
		{
			double x1 = Math.Cos(angle) * this.Radius;
			double z1 = Math.Sin(angle) * this.Radius;
			angle += angleStep;
			double x2 = Math.Cos(angle) * this.Radius;
			double z2 = Math.Sin(angle) * this.Radius;

			this.Points.Add(new Point3D(x1, 0.0, z1));
			this.Points.Add(new Point3D(x2, 0.0, z2));
		}
	}
}
