// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Math3D.Extensions;

using System;
using System.Runtime.CompilerServices;

public static class FloatExtensions
{
	/// <summary>
	/// Determines whether two floating-point numbers are approximately equal within a specified error margin.
	/// </summary>
	/// <param name="a">The first floating-point number.</param>
	/// <param name="b">The second floating-point number.</param>
	/// <param name="errorMargin">The acceptable error margin for the comparison.</param>
	/// <returns>True if the absolute difference between the two numbers is less than the error margin; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsApproximately(this float a, float b, float errorMargin) => MathF.Abs(a - b) < errorMargin;
}
