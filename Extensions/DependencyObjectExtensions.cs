// © XIV-Tools.
// Licensed under the MIT license.

#pragma warning disable IDE0130
namespace System.Windows;
#pragma warning restore IDE0130

using System.Collections.Generic;
using System.Windows.Media;

public static class DependencyObjectExtensions
{
	public static T? FindParent<T>(this DependencyObject child)
		where T : DependencyObject
	{
		DependencyObject parentObject = VisualTreeHelper.GetParent(child);

		if (parentObject == null)
			return null;

		if (parentObject is T parent)
		{
			return parent;
		}
		else
		{
			return parentObject.FindParent<T>();
		}
	}

	public static T? FindChild<T>(this DependencyObject self)
		where T : notnull
	{
		var results = new List<T>();
		self.FindChildren<T>(ref results);

		if (results.Count == 0)
			return default;

		return results[0];
	}

	public static List<T> FindChildren<T>(this DependencyObject self)
	{
		var results = new List<T>();
		self.FindChildren<T>(ref results);
		return results;
	}

	public static void FindChildren<T>(this DependencyObject self, ref List<T> results)
	{
		int children = VisualTreeHelper.GetChildrenCount(self);
		for (int i = 0; i < children; i++)
		{
			DependencyObject? child = VisualTreeHelper.GetChild(self, i);

			if (child is T tChild)
				results.Add(tChild);

			child.FindChildren(ref results);
		}
	}
}
