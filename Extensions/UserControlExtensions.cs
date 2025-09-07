// © XIV-Tools.
// Licensed under the MIT license.

#pragma warning disable IDE0130
namespace System.Windows.Controls;
#pragma warning restore IDE0130

using System.Windows;

public static class UserControlExtensions
{
	public static T GetValue<T>(this UserControl self, DependencyProperty dp)
	{
		return (T)self.GetValue(dp);
	}
}
