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
	/// Multiplies a quaternion by a vector, applying the rotation represented by the quaternion to the vector.
	/// </summary>
	/// <param name="left">The quaternion representing the rotation.</param>
	/// <param name="right">The vector to be rotated.</param>
	/// <returns>A new vector that is the result of applying the quaternion rotation to the input vector.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Multiply(Quaternion left, Vector3 right)
	{
		float num = left.X + left.X;
		float num2 = left.Y + left.Y;
		float num3 = left.Z + left.Z;
		float num4 = left.X * num;
		float num5 = left.Y * num2;
		float num6 = left.Z * num3;
		float num7 = left.X * num2;
		float num8 = left.X * num3;
		float num9 = left.Y * num3;
		float num10 = left.W * num;
		float num11 = left.W * num2;
		float num12 = left.W * num3;
		float x = ((1f - (num5 + num6)) * right.X) + ((num7 - num12) * right.Y) + ((num8 + num11) * right.Z);
		float y = ((num7 + num12) * right.X) + ((1f - (num4 + num6)) * right.Y) + ((num9 - num10) * right.Z);
		float z = ((num8 - num11) * right.X) + ((num9 + num10) * right.Y) + ((1f - (num4 + num5)) * right.Z);
		return new Vector3(x, y, z);
	}

	/// <summary>
	/// Inverts the quaternion, producing its conjugate.
	/// </summary>
	/// <param name="self">The quaternion to be inverted.</param>
	public static void Invert(this Quaternion self)
	{
		self = Quaternion.Inverse(self);
	}

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

	/// <summary>
	/// Returns the maximum value among four floating-point numbers.
	/// </summary>
	/// <param name="a">The first floating-point number.</param>
	/// <param name="b">The second floating-point number.</param>
	/// <param name="c">The third floating-point number.</param>
	/// <param name="d">The fourth floating-point number.</param>
	/// <returns>The maximum value among the four numbers.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Max(float a, float b, float c, float d) => MathF.Max(MathF.Max(a, b), MathF.Max(c, d));
}
