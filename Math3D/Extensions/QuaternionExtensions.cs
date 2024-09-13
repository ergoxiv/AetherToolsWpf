// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D.Extensions;

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class QuaternionExtensions
{
	private static readonly float Deg2Rad = MathF.PI / 180f;
	private static readonly float Rad2Deg = 180f / MathF.PI;

	/// <summary>
	/// Converts a string representation of a quaternion to a Quaternion object.
	/// </summary>
	/// <param name="str">The string representation of the quaternion, with components separated by ", ".</param>
	/// <returns>A Quaternion object parsed from the string.</returns>
	/// <exception cref="FormatException">Thrown if the string does not contain exactly four components.</exception>
	public static Quaternion FromString(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException("The provided string does not have four components");

		return new Quaternion(
			float.Parse(parts[0], CultureInfo.InvariantCulture),
			float.Parse(parts[1], CultureInfo.InvariantCulture),
			float.Parse(parts[2], CultureInfo.InvariantCulture),
			float.Parse(parts[3], CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Converts the quaternion to a string representation with components formatted using invariant culture.
	/// </summary>
	/// <param name="quaternion">The quaternion to be converted to a string.</param>
	/// <returns>
	/// A string representation of the quaternion with components separated by ", " and formatted using
	/// invariant culture.
	/// </returns>
	public static string ToInvariantString(this Quaternion quaternion)
	{
		return quaternion.X.ToString(CultureInfo.InvariantCulture) + ", "
			+ quaternion.Y.ToString(CultureInfo.InvariantCulture) + ", "
			+ quaternion.Z.ToString(CultureInfo.InvariantCulture) + ", "
			+ quaternion.W.ToString(CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts Euler angles (in degrees) to a quaternion.
	/// </summary>
	/// <param name="euler">A Vector3 representing the Euler angles (in degrees).</param>
	/// <returns>A quaternion representing the same rotation as the Euler angles.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion FromEuler(Vector3 euler)
	{
		double yaw = euler.Y * Deg2Rad;
		double pitch = euler.X * Deg2Rad;
		double roll = euler.Z * Deg2Rad;

		double c1 = Math.Cos(yaw / 2);
		double s1 = Math.Sin(yaw / 2);
		double c2 = Math.Cos(pitch / 2);
		double s2 = Math.Sin(pitch / 2);
		double c3 = Math.Cos(roll / 2);
		double s3 = Math.Sin(roll / 2);

		double c1c2 = c1 * c2;
		double s1s2 = s1 * s2;

		return new Quaternion(
			(float)((c1c2 * s3) + (s1s2 * c3)),
			(float)((s1 * c2 * c3) + (c1 * s2 * s3)),
			(float)((c1 * s2 * c3) - (s1 * c2 * s3)),
			(float)((c1c2 * c3) - (s1s2 * s3)));
	}

	/// <summary>
	/// Converts a quaternion to Euler angles (in degrees).
	/// </summary>
	/// <param name="quaternion">The quaternion to be converted.</param>
	/// <returns>A Vector3 representing the Euler angles (in degrees) corresponding to the quaternion.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ToEuler(this Quaternion quaternion)
	{
		Vector3 v = default;

		double test = (quaternion.X * quaternion.Y) + (quaternion.Z * quaternion.W);

		if (test > 0.4995f)
		{
			v.Y = 2f * MathF.Atan2(quaternion.X, quaternion.Y);
			v.X = MathF.PI / 2;
			v.Z = 0;
		}
		else if (test < -0.4995f)
		{
			v.Y = -2f * MathF.Atan2(quaternion.X, quaternion.W);
			v.X = -MathF.PI / 2;
			v.Z = 0;
		}
		else
		{
			double sqx = quaternion.X * quaternion.X;
			double sqy = quaternion.Y * quaternion.Y;
			double sqz = quaternion.Z * quaternion.Z;

			v.Y = (float)Math.Atan2((2 * quaternion.Y * quaternion.W) - (2 * quaternion.X * quaternion.Z), 1 - (2 * sqy) - (2 * sqz));
			v.X = (float)Math.Asin(2 * test);
			v.Z = (float)Math.Atan2((2 * quaternion.X * quaternion.W) - (2 * quaternion.Y * quaternion.Z), 1 - (2 * sqx) - (2 * sqz));
		}

		v *= Rad2Deg;
		VectorExtensions.NormalizeAngles(ref v);
		return v;
	}

	/// <summary>
	/// Converts a System.Numerics quaternion to a Media3D quaternion.
	/// </summary>
	/// <param name="self">The System.Numerics quaternion to be converted.</param>
	/// <returns>A new Media3D quaternion with the same components as the System.Numerics quaternion.</returns>
	public static System.Windows.Media.Media3D.Quaternion ToMedia3DQuaternion(this Quaternion self)
	{
		return new System.Windows.Media.Media3D.Quaternion(self.X, self.Y, self.Z, self.W);
	}

	/// <summary>
	/// Converts a Media3D quaternion to a System.Numerics quaternion.
	/// </summary>
	/// <param name="self">The Media3D quaternion to be converted.</param>
	/// <returns>A new System.Numerics quaternion with the same components as the Media3D quaternion.</returns>
	public static Quaternion FromMedia3DQuaternion(this System.Windows.Media.Media3D.Quaternion self)
	{
		return new Quaternion((float)self.X, (float)self.Y, (float)self.Z, (float)self.W);
	}

	/// <summary>
	/// Mirrors the target quaternion.
	/// </summary>
	/// <param name="self">The quaternion to be mirrored.</param>
	/// <returns>A new quaternion with its components rearranged.</returns>
	public static Quaternion Mirror(this Quaternion self)
	{
		return new Quaternion(self.Z, self.W, self.X, self.Y);
	}

	/// <summary>
	/// Determines whether two quaternions are approximately equal within a specified error margin.
	/// </summary>
	/// <param name="lhs">The first quaternion.</param>
	/// <param name="rhs">The second quaternion.</param>
	/// <param name="errorMargin">The acceptable error margin for the comparison. Default is 0.001f.</param>
	/// <returns>True if the absolute differences between the corresponding components of the two quaternions are all less than the error margin; otherwise, false.</returns>
	public static bool IsApproximately(this Quaternion lhs, Quaternion rhs, float errorMargin = 0.001f)
	{
		return IsApproximately(lhs.X, rhs.X, errorMargin)
			&& IsApproximately(lhs.Y, rhs.Y, errorMargin)
			&& IsApproximately(lhs.Z, rhs.Z, errorMargin)
			&& IsApproximately(lhs.W, rhs.W, errorMargin);
	}

	/// <summary>
	/// Returns the hash code for the specified quaternion.
	/// </summary>
	/// <param name="quaternion">The quaternion for which to compute the hash code.</param>
	/// <returns>A hash code for the specified quaternion.</returns>
	public static int GetHashCode(this Quaternion quaternion)
	{
		return HashCode.Combine(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
	}

	/// <summary>
	/// Determines whether two floating-point numbers are approximately equal within a specified error margin.
	/// </summary>
	/// <param name="a">The first floating-point number.</param>
	/// <param name="b">The second floating-point number.</param>
	/// <param name="errorMargin">The acceptable error margin for the comparison.</param>
	/// <returns>True if the absolute difference between the two numbers is less than the error margin; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsApproximately(float a, float b, float errorMargin) => MathF.Abs(a - b) < errorMargin;
}
