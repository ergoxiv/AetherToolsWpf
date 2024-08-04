// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Utility;

using System.Windows;

/// <summary>
/// A proxy class that allows binding to data in XAML where direct binding is not possible.
/// </summary>
public class BindingProxy : Freezable
{
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

	/// <summary>
	/// Gets or sets the data to be used for binding.
	/// </summary>
	public object Data
	{
		get { return (object)this.GetValue(DataProperty); }
		set { this.SetValue(DataProperty, value); }
	}

	/// <summary>
	/// Creates a new instance of the <see cref="BindingProxy"/> class.
	/// </summary>
	/// <returns>A new instance of the <see cref="BindingProxy"/> class.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new BindingProxy();
	}
}
