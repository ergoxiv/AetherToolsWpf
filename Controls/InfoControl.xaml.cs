// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using PropertyChanged;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for InfoControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class InfoControl : UserControl
{
	public static readonly IBind<string?> KeyDp = Binder.Register<string?, InfoControl>(nameof(Key), OnKeyChanged, BindMode.OneWay);

	public InfoControl()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
		this.IsError = false;
	}

	public string? Key
	{
		get => KeyDp.Get(this);
		set
		{
			KeyDp.Set(this, value);
			this.TextBlock.Key = value;
		}
	}

	public string? Text
	{
		get => this.TextBlock.Text;
		set => this.TextBlock.Text = value;
	}

	public bool IsError { get; set; }

	private static void OnKeyChanged(InfoControl sender, string? newValue)
	{
		sender.TextBlock.Key = newValue;
	}
}
