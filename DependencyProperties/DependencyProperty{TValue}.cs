// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyProperties;

using System.Windows;

public class DependencyProperty<TValue>(DependencyProperty dp) : IBind<TValue>
{
	private readonly DependencyProperty dp = dp;

	public TValue Get(DependencyObject control) => (TValue)control.GetValue(this.dp);

	public void Set(DependencyObject control, TValue value)
	{
		TValue old = this.Get(control);

		if (old != null && old.Equals(value))
			return;

		control.SetValue(this.dp, value);
	}
}
