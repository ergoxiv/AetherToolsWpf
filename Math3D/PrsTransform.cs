// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D;

using System.Windows.Media.Media3D;

/// <summary>
/// Represents a Position, Rotation, and Scale (PRS) transform.
/// </summary>
public class PrsTransform
{
	private readonly Transform3DGroup transform = new();
	private readonly TranslateTransform3D position = new();
	private readonly QuaternionRotation3D rotation = new();
	private readonly ScaleTransform3D scale = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="PrsTransform"/> class.
	/// </summary>
	public PrsTransform()
	{
		this.transform.Children.Add(this.position);
		this.transform.Children.Add(new RotateTransform3D { Rotation = this.rotation });
		this.transform.Children.Add(this.scale);
	}

	/// <summary>Gets the combined transform group.</summary>
	public Transform3DGroup Transform => this.transform;

	/// <summary>Gets a value indicating whether the transform is affine.</summary>
	public bool IsAffine => this.transform.IsAffine;

	/// <summary>Gets the matrix that represents the transform.</summary>
	public Matrix3D Value => this.transform.Value;

	/// <summary>Gets or sets the scale as a <see cref="Vector3D"/>.</summary>
	public Vector3D Scale3D
	{
		get => new(this.scale.ScaleX, this.scale.ScaleY, this.scale.ScaleZ);
		set
		{
			this.scale.ScaleX = value.X;
			this.scale.ScaleY = value.Y;
			this.scale.ScaleZ = value.Z;
		}
	}

	/// <summary>Gets or sets the uniform scale.</summary>
	public double UniformScale
	{
		get => this.scale.ScaleX;
		set
		{
			this.scale.ScaleX = value;
			this.scale.ScaleY = value;
			this.scale.ScaleZ = value;
		}
	}

	/// <summary>Gets or sets the rotation as a <see cref="Quaternion"/>.</summary>
	public Quaternion Rotation
	{
		get => this.rotation.Quaternion;
		set => this.rotation.Quaternion = value;
	}

	/// <summary>Gets or sets the position as a <see cref="Vector3D"/>.</summary>
	public Vector3D Position
	{
		get => new(this.position.OffsetX, this.position.OffsetY, this.position.OffsetZ);
		set
		{
			this.position.OffsetX = value.X;
			this.position.OffsetY = value.Y;
			this.position.OffsetZ = value.Z;
		}
	}

	/// <summary>
	/// Resets the transform to the default position, rotation, and scale.
	/// </summary>
	public void Reset()
	{
		this.Position = new Vector3D(0, 0, 0);
		this.Rotation = Quaternion.Identity;
		this.UniformScale = 1;
	}
}
