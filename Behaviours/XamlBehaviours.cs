// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Behaviours;

using System;
using System.Runtime.CompilerServices;
using System.Windows;

public static class XamlBehaviours
{
	private static readonly ConditionalWeakTable<DependencyObject, Behaviour> AttachedHandlers = [];

	public static void AttachHandler<T>(this DependencyObject element, bool enable, object? value = null)
		where T : Behaviour
	{
		if (enable)
		{
			if (!AttachedHandlers.TryGetValue(element, out var handler))
			{
				if (value == null)
				{
					handler = Activator.CreateInstance(typeof(T), [element]) as Behaviour;
				}
				else
				{
					handler = Activator.CreateInstance(typeof(T), [element, value]) as Behaviour;
				}

				if (handler == null)
					throw new InvalidOperationException();

				AttachedHandlers.Add(element, handler);
			}
		}
		else
		{
			if (!AttachedHandlers.TryGetValue(element, out var handler))
				return;

			handler.Dispose();
			AttachedHandlers.Remove(element);
		}
	}
}