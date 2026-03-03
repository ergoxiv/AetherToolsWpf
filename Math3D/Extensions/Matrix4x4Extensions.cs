// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D.Extensions;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

public static class Matrix4x4Extensions
{
	/// <summary>
	/// Determines whether two matrices are approximately equal within a specified error margin.
	/// </summary>
	/// <remarks>
	/// This method is useful for comparing matrices when minor floating-point inaccuracies are
	/// expected. It performs an element-wise comparison using the specified error margin.
	/// </remarks>
	/// <param name="lhs">The first matrix to compare.</param>
	/// <param name="rhs">The second matrix to compare.</param>
	/// <param name="errorMargin">
	/// The maximum allowed difference between corresponding elements for the matrices to be
	/// considered approximately equal. Must be greater than or equal to 0. The default is 0.0001.
	/// </param>
	/// <returns>
	/// true if all corresponding elements of the matrices differ by no more than the
	/// specified error margin; otherwise, false.
	/// </returns>
	public static bool IsApproximately(this Matrix4x4 lhs, Matrix4x4 rhs, float errorMargin = 0.0001f)
	{
		return lhs.M11.IsApproximately(rhs.M11, errorMargin)
			&& lhs.M12.IsApproximately(rhs.M12, errorMargin)
			&& lhs.M13.IsApproximately(rhs.M13, errorMargin)
			&& lhs.M14.IsApproximately(rhs.M14, errorMargin)
			&& lhs.M21.IsApproximately(rhs.M21, errorMargin)
			&& lhs.M22.IsApproximately(rhs.M22, errorMargin)
			&& lhs.M23.IsApproximately(rhs.M23, errorMargin)
			&& lhs.M24.IsApproximately(rhs.M24, errorMargin)
			&& lhs.M31.IsApproximately(rhs.M31, errorMargin)
			&& lhs.M32.IsApproximately(rhs.M32, errorMargin)
			&& lhs.M33.IsApproximately(rhs.M33, errorMargin)
			&& lhs.M34.IsApproximately(rhs.M34, errorMargin)
			&& lhs.M41.IsApproximately(rhs.M41, errorMargin)
			&& lhs.M42.IsApproximately(rhs.M42, errorMargin)
			&& lhs.M43.IsApproximately(rhs.M43, errorMargin)
			&& lhs.M44.IsApproximately(rhs.M44, errorMargin);
	}

	/// <summary>
	/// Converts a Matrix3D to a Matrix4x4.
	/// </summary>
	/// <param name="m">
	/// The matrix to convert.
	/// </param>
	/// <returns>
	/// The converted Matrix4x4.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 ToMatrix4x4(this Matrix3D m)
	{
		return new Matrix4x4(
			(float)m.M11, (float)m.M12, (float)m.M13, (float)m.M14,
			(float)m.M21, (float)m.M22, (float)m.M23, (float)m.M24,
			(float)m.M31, (float)m.M32, (float)m.M33, (float)m.M34,
			(float)m.OffsetX, (float)m.OffsetY, (float)m.OffsetZ, (float)m.M44);
	}

	/// <summary>
	/// Converts a Matrix4x4 to a Matrix3D.
	/// </summary>
	/// <param name="m">
	/// The matrix to convert.
	/// </param>
	/// <returns>
	/// The converted Matrix3D.
	/// </returns>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D ToMatrix3D(this Matrix4x4 m)
	{
		return new Matrix3D(
			m.M11, m.M12, m.M13, m.M14,
			m.M21, m.M22, m.M23, m.M24,
			m.M31, m.M32, m.M33, m.M34,
			m.M41, m.M42, m.M43, m.M44);
	}
}

