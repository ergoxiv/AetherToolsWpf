// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyProperties;

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

public class Binder
{
	public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, BindMode mode)
	{
		static void callback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
		return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback((Action<DependencyObject, DependencyPropertyChangedEventArgs>)callback), mode);
	}

	public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue>? changed = null, BindMode mode = BindMode.TwoWay)
	{
		void callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TOwner owner)
			{
				changed?.Invoke(owner, (TValue)e.NewValue);
			}
		}

		return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback((Action<DependencyObject, DependencyPropertyChangedEventArgs>)callback), mode);
	}

	public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue, TValue> changed, BindMode mode = BindMode.TwoWay)
	{
		void callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TOwner owner)
			{
				changed?.Invoke(owner, (TValue)e.OldValue, (TValue)e.NewValue);
			}
		}

		return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback((Action<DependencyObject, DependencyPropertyChangedEventArgs>)callback), mode);
	}

	private static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, PropertyChangedCallback callback, BindMode mode)
	{
		PropertyInfo? property = typeof(TOwner).GetProperty(propertyName)
			?? throw new Exception("Failed to locate property: \"" + propertyName + "\" on type: \"" + typeof(TOwner) + "\" for binding.");

		var meta = new FrameworkPropertyMetadata(new PropertyChangedCallback(callback))
		{
			BindsTwoWayByDefault = mode == BindMode.TwoWay,
			DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			Inherits = true
		};

		DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), meta);
		var dpv = new DependencyProperty<TValue>(dp);
		return dpv;
	}
}

public enum BindMode
{
	OneWay,
	TwoWay,
}
