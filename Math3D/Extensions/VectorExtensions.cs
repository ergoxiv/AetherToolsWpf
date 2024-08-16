// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D.Extensions;

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class VectorExtensions
{
	/// <summary>
	/// Determines whether two 3D vectors are approximately equal within a specified error margin.
	/// </summary>
	/// <param name="lhs">The first 3D vector.</param>
	/// <param name="rhs">The second 3D vector.</param>
	/// <param name="errorMargin">The acceptable error margin for the comparison. Default is 0.001f.</param>
	/// <returns>
	/// True if the absolute differences between the corresponding components of the two vectors are all
	/// less than the error margin; otherwise, false.
	/// </returns>
	public static bool IsApproximately(this Vector3 lhs, Vector3 rhs, float errorMargin = 0.001f)
	{
		return IsApproximately(lhs.X, rhs.X, errorMargin)
			&& IsApproximately(lhs.Y, rhs.Y, errorMargin)
			&& IsApproximately(lhs.Z, rhs.Z, errorMargin);
	}

	/// <summary>
	/// Adds a scalar value to each component of a 3D vector.
	/// </summary>
	/// <param name="left">The 3D vector to which the scalar value will be added.</param>
	/// <param name="right">The scalar value to be added to each component of the vector.</param>
	/// <returns>A new Vector3 with the scalar value added to each component.</returns>
	public static Vector3 Add(Vector3 left, float right)
	{
		return new Vector3(left.X + right, left.Y + right, left.Z + right);
	}

	/// <summary>
	/// Multiplies each component of a 3D vector by a scalar value.
	/// </summary>
	/// <param name="left">The 3D vector whose components will be multiplied by the scalar value.</param>
	/// <param name="right">The scalar value by which to multiply each component of the vector.</param>
	/// <returns>A new Vector3 with each component multiplied by the scalar value.</returns>
	public static Vector3 Multiply(Vector3 left, float right)
	{
		return new Vector3(left.X * right, left.Y * right, left.Z * right);
	}

	/// <summary>
	/// Converts a string representation of a 3D vector to a Vector3 object.
	/// </summary>
	/// <param name="str">The string representation of the 3D vector, with components separated by ", ".</param>
	/// <returns>A Vector3 object parsed from the string.</returns>
	/// <exception cref="FormatException">Thrown if the string does not contain exactly three components.</exception>
	public static Vector3 FromString3D(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 3)
			throw new FormatException();

		float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
		float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		return new Vector3(x, y, z);
	}

	/// <summary>
	/// Converts a string representation of a 2D vector to a Vector2 object.
	/// </summary>
	/// <param name="str">The string representation of the 2D vector, with components separated by ", ".</param>
	/// <returns>A Vector2 object parsed from the string.</returns>
	/// <exception cref="FormatException">Thrown if the string does not contain exactly two components.</exception>
	public static Vector2 FromString2D(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 3)
			throw new FormatException();

		Vector2 v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		return v;
	}

	/// <summary>
	/// Converts a System.Numerics Vector3 to a Media3D Vector3D.
	/// </summary>
	/// <param name="self">The System.Numerics Vector3 to be converted.</param>
	/// <returns>A new Media3D Vector3D with the same components as the System.Numerics Vector3.</returns>
	public static System.Windows.Media.Media3D.Vector3D ToMedia3DVector(this Vector3 self)
	{
		return new System.Windows.Media.Media3D.Vector3D(self.X, self.Y, self.Z);
	}

	/// <summary>
	/// Converts a Media3D Vector3D to a System.Numerics Vector3.
	/// </summary>
	/// <param name="self">The Media3D Vector3D to be converted.</param>
	/// <returns>A new System.Numerics Vector3 with the same components as the Media3D Vector3D.</returns>
	public static Vector3 FromMedia3DQuaternion(this System.Windows.Media.Media3D.Vector3D self)
	{
		return new Vector3((float)self.X, (float)self.Y, (float)self.Z);
	}

	public static System.Windows.Media.Media3D.Point3D ToMedia3DPoint(this Vector3 self)
	{
		return new System.Windows.Media.Media3D.Point3D(self.X, self.Y, self.Z);
	}

	public static Vector3 FromMedia3DPoint(this System.Windows.Media.Media3D.Point3D self)
	{
		return new Vector3((float)self.X, (float)self.Y, (float)self.Z);
	}

	/// <summary>
	/// Determines whether the components of a nullable 3D vector are valid (i.e., not null, not infinity, and not NaN).
	/// </summary>
	/// <param name="vec">The nullable 3D vector to be validated.</param>
	/// <returns>True if all components of the vector are valid; otherwise, false.</returns>
	public static bool IsValid(this Vector3? vec)
	{
		if (vec == null)
			return false;

		Vector3 v = (Vector3)vec;
		return VectorExtensions.IsValid(v);
	}

	/// <summary>
	/// Normalizes the angles of a 3D vector to be within the range of 0 to 360 degrees.
	/// </summary>
	/// <param name="vector">The 3D vector whose angles are to be normalized.</param>
	public static void NormalizeAngles(ref Vector3 vector)
	{
		vector.X = NormalizeAngle(vector.X);
		vector.Y = NormalizeAngle(vector.Y);
		vector.Z = NormalizeAngle(vector.Z);
	}

	/// <summary>
	/// Determines whether the components of a 3D vector are valid (i.e., not null, not infinity and not NaN).
	/// </summary>
	/// <param name="vector">The 3D vector to be validated.</param>
	/// <returns>True if all components of the vector are valid; otherwise, false.</returns>
	public static bool IsValid(this Vector3 vector)
	{
		bool valid = IsValid(vector.X);
		valid &= IsValid(vector.Y);
		valid &= IsValid(vector.Z);

		return valid;
	}

	/// <summary>
	/// Returns the hash code for the specified 3D vector.
	/// </summary>
	/// <param name="vector">The 3D vector for which to compute the hash code.</param>
	/// <returns>A hash code for the specified 3D vector.</returns>
	public static int GetHashCode(ref Vector3 vector)
	{
		return HashCode.Combine(vector.X, vector.Y, vector.Z);
	}

	/// <summary>
	/// Returns the hash code for the specified 2D vector.
	/// </summary>
	/// <param name="vector">The 2D vector for which to compute the hash code.</param>
	/// <returns>A hash code for the specified 2D vector.</returns>
	public static int GetHashCode(ref Vector2 vector)
	{
		return HashCode.Combine(vector.X, vector.Y);
	}

	/// <summary>
	/// Normalizes an angle to be within the range of 0 to 360 degrees.
	/// </summary>
	/// <param name="angle">The angle to be normalized.</param>
	/// <returns>The normalized angle within the range of 0 to 360 degrees.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float NormalizeAngle(float angle)
	{
		angle %= 360;
		if (angle < 0)
			angle += 360;
		return angle;
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
	/// Determines whether a nullable float is valid (i.e., not null, not infinity, and not NaN).
	/// </summary>
	/// <param name="number">The nullable float to be validated.</param>
	/// <returns>True if the number is not null, not infinity, and not NaN; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsValid(float? number)
	{
		if (number == null)
			return false;

		float v = (float)number;

		if (float.IsInfinity(v) || float.IsNaN(v))
			return false;

		return true;
	}
}
