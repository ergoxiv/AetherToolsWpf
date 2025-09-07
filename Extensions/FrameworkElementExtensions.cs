// © XIV-Tools.
// Licensed under the MIT license.

#pragma warning disable IDE0130
namespace System.Windows;
#pragma warning restore IDE0130

using System;
using System.Windows.Media.Animation;

public static class FrameworkElementExtensions
{
	public static void Animate(this FrameworkElement self, DependencyProperty property, double to, int durationMs)
	{
		var story = new Storyboard();
		var anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(durationMs)));
		Storyboard.SetTarget(anim, self);
		Storyboard.SetTargetProperty(anim, new PropertyPath(property));
		story.Children.Add(anim);
		story.Begin();
	}
}
