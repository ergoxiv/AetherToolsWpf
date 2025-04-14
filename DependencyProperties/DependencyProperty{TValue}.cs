// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyProperties;

using System.Runtime.CompilerServices;
using System.Windows;

public class DependencyProperty<TValue>(DependencyProperty dp) : IBind<TValue>
{
	private readonly DependencyProperty dp = dp;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue Get(DependencyObject control) => (TValue)control.GetValue(this.dp);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(DependencyObject control, TValue value) => control.SetValue(this.dp, value);
}
