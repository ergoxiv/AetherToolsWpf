﻿// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using XivToolsWpf.DependencyProperties;
	using XivToolsWpf.Services;

	/// <summary>
	/// Interaction logic for Label.xaml.
	/// </summary>
	public partial class TextBlock : System.Windows.Controls.TextBlock
	{
		public static readonly IBind<string> KeyDp = Binder.Register<string, TextBlock>(nameof(Key), OnKeyChanged, BindMode.OneWay);
		public static readonly IBind<string?> ValueDp = Binder.Register<string?, TextBlock>(nameof(Value), OnValueChanged, BindMode.OneWay);

		public TextBlock()
		{
			this.Loaded += this.TextBlock_Loaded;
			LocalizationService.LocaleChanged += this.OnLocaleChanged;
		}

		public string? Key { get; set; }

		public string? Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		public static void OnKeyChanged(TextBlock sender, string val)
		{
			sender.Key = val;
			sender.LoadString();
		}

		public static void OnValueChanged(TextBlock sender, string? val)
		{
			sender.LoadString();
		}

		private void TextBlock_Loaded(object sender, RoutedEventArgs e)
		{
			this.LoadString();
		}

		private void OnLocaleChanged()
		{
			this.LoadString();
		}

		private void LoadString()
		{
			if (string.IsNullOrEmpty(this.Key))
				return;

			string? val = null;

			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				if (this.Value == null)
				{
					val = LocalizationService.GetString(this.Key);
				}
				else
				{
					val = LocalizationService.GetStringFormatted(this.Key, this.Value);
				}
			}

			if (string.IsNullOrEmpty(val))
			{
				if (!string.IsNullOrEmpty(this.Text))
					return;

				val = "[" + this.Key + "]";
			}

			this.Text = val;
		}
	}
}
