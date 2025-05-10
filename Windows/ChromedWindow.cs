// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Windows;

using MaterialDesignThemes.Wpf;
using PropertyChanged;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using XivToolsWpf.Utility;

[AddINotifyPropertyChangedInterface]
public class ChromedWindow : Window
{
	private bool enableTranslucency = true;
	private bool isDarkTheme = false;
	private bool extendIntoChrome = true;

	public ChromedWindow()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);

		this.SetChrome();

		this.Style = Application.Current.FindResource("ChromedWindowStyle") as Style;

		this.MouseDown += this.OnMouseDown;
		this.Loaded += this.OnLoaded;

		IThemeManager? themeManager = new PaletteHelper().GetThemeManager();
		if (themeManager != null)
		{
			themeManager.ThemeChanged += this.OnThemeChanged;
		}
	}

	public bool EnableTranslucency
	{
		get => this.enableTranslucency;
		set
		{
			this.enableTranslucency = value;
			this.SetTranslucency();
		}
	}

	public bool TransprentWhenNotInFocus { get; set; }
	public double WindowOpacity { get; set; }

	public bool ExtendIntoChrome
	{
		get => this.extendIntoChrome;
		set
		{
			this.extendIntoChrome = value;
			this.SetChrome();
		}
	}

	public bool GetIsActive() => WindowExtensions.GetIsActive(this);

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		this.SetTranslucency();
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);
		this.SetTranslucency();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.SetTranslucency();
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.Handled)
			return;

		if (e.ChangedButton != MouseButton.Left)
			return;

		if (e.LeftButton != MouseButtonState.Pressed)
			return;

		IInputElement? element = Mouse.DirectlyOver;

		if (element is FrameworkElement obj)
		{
			if (obj.Name == "TitleBarArea" || obj.Parent == null)
			{
				this.DragMove();
			}
		}
	}

	private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
	{
		this.SetChrome();
		this.SetTranslucency();

		// Force a redraw
		Win32.RedrawNonClientArea(this);
		this.InvalidateVisual();
		this.UpdateLayout();
	}

	private void SetChrome()
	{
		WindowChrome? chrome = WindowChrome.GetWindowChrome(this);

		if (chrome is null)
		{
			chrome = new WindowChrome();
			WindowChrome.SetWindowChrome(this, chrome);
		}

		if (this.extendIntoChrome)
		{
			chrome.NonClientFrameEdges = NonClientFrameEdges.Right | NonClientFrameEdges.Left | NonClientFrameEdges.Bottom;
			chrome.CaptionHeight = 0;
			chrome.GlassFrameThickness = new Thickness(-1);
		}
		else
		{
			chrome.NonClientFrameEdges = NonClientFrameEdges.None;
			chrome.CaptionHeight = 22;
			chrome.GlassFrameThickness = default;
			chrome.UseAeroCaptionButtons = true;
		}
	}

	private void SetTranslucency()
	{
		if (!this.ExtendIntoChrome)
			this.enableTranslucency = false;

		if (this.GetTemplateChild("TitleBarArea") is not Rectangle titlebarRect ||
			this.GetTemplateChild("BackgroundArea") is not Rectangle backgroundRect)
			return;

		WindowInteropHelper? windowHelper = new(this);
		bool enableBlurEffect = false;

		this.isDarkTheme = new PaletteHelper().GetTheme().GetBaseTheme() == BaseTheme.Dark;

		Win32.SetWindowDarkMode(windowHelper.Handle, this.isDarkTheme);

		int blurOpacity = 0;
		int blurBackgroundColor = 0x000000;

		bool isWindows11 = Win32.IsWindows11();
		bool isWindows10 = Win32.IsWindows10();

		var accent = new Win32.AccentPolicy();

		if (this.TransprentWhenNotInFocus && !this.IsActive)
		{
			accent.AccentState = Win32.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
			blurOpacity = (int)Math.Clamp(255 * this.WindowOpacity, 0, 255);
			blurBackgroundColor = this.isDarkTheme ? 0x303030 : 0xFFFFFF;
		}
		else if (!this.EnableTranslucency || (!isWindows10 && !isWindows11))
		{
			accent.AccentState = Win32.AccentState.ACCENT_DISABLED;
			backgroundRect.Visibility = Visibility.Visible;
			backgroundRect.Opacity = 1.0;
			titlebarRect.Fill = new SolidColorBrush(Colors.Transparent);
			titlebarRect.Opacity = 1.0;
		}
		else if (isWindows11)
		{
			accent.AccentState = Win32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
			blurOpacity = this.isDarkTheme ? 210 : 150;
			blurBackgroundColor = this.isDarkTheme ? 0x202020 : 0xFFFFFF;

			backgroundRect.Visibility = Visibility.Collapsed;
			titlebarRect.Fill = new SolidColorBrush(Colors.Transparent);
			titlebarRect.Opacity = 1.0;

			enableBlurEffect = true;

			// Set system-drawn backdrop type to Acrylic (Windows 11 Build 22621 and higher)
			Win32.SetSystemBackdropType(windowHelper.Handle, Win32.DwmSystemBackdropType.DWMSBT_TRANSIENTWINDOW);
		}
		else if (isWindows10)
		{
			accent.AccentState = Win32.AccentState.ACCENT_ENABLE_BLURBEHIND;
			blurOpacity = 255;
			blurBackgroundColor = 0x000000;
			backgroundRect.Visibility = Visibility.Visible;
			backgroundRect.Opacity = 0.75;
			titlebarRect.Fill = Application.Current.FindResource("MaterialDesignPaper") as SolidColorBrush;
			titlebarRect.Opacity = 0.75;
			enableBlurEffect = true;
		}

		accent.GradientColor = ((uint)blurOpacity << 24) | ((uint)blurBackgroundColor & 0xFFFFFF);

		Win32.SetAccentPolicy(windowHelper.Handle, accent);

		Win32.SetBlurBehindWindow(windowHelper.Handle, enableBlurEffect);
	}
}
