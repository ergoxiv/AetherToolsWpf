// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf;

using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

public static class Themes
{
	private static Color currentColor;
	private static bool currentLight;
	private static int? prevThemeRegKey = null;

	static Themes()
	{
		SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
	}

	public static bool IsLightTheme => currentLight;

	public static void ApplyCustomTheme(bool light, Color highlightColor)
	{
		if (currentColor == highlightColor && currentLight == light)
			return;

		currentColor = highlightColor;
		currentLight = light;

		var theme = new Theme();

		if (light)
		{
			var lightTheme = new LightTheme();
			theme.SetBaseTheme(lightTheme);

			// Custom theme properties (Extension from material design base theme)
			Application.Current.Resources["PanelBackground"] = lightTheme.PanelBackground;
			Application.Current.Resources["PanelBackgroundBrush"] = new SolidColorBrush(lightTheme.PanelBackground);

		}
		else
		{
			var darkTheme = new DarkTheme();
			theme.SetBaseTheme(darkTheme);

			// Custom theme properties (Extension from material design base theme)
			Application.Current.Resources["PanelBackground"] = darkTheme.PanelBackground;
			Application.Current.Resources["PanelBackgroundBrush"] = new SolidColorBrush(darkTheme.PanelBackground);
		}

		theme.SetPrimaryColor(currentColor);
		////theme.SetSecondaryColor(currentColor);

		Application.Current.Resources.SetTheme(theme);
	}

	public static void ApplySystemTheme()
	{
		int? currentValue = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\\", "AppsUseLightTheme", 0) as int?;

		if (currentValue != prevThemeRegKey)
		{
			prevThemeRegKey = currentValue;

			if (currentValue != null)
			{
				bool lightMode = currentValue == 1;
				ApplyCustomTheme(lightMode, SystemParameters.WindowGlassColor);
			}
		}
	}

	private static void OnSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		ApplySystemTheme();
	}

	public class DarkTheme : IBaseTheme
	{
		public Color MaterialDesignValidationErrorColor { get; } = (Color)ColorConverter.ConvertFromString("#f44336");
		public Color MaterialDesignBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF202020");
		public Color MaterialDesignPaper { get; } = (Color)ColorConverter.ConvertFromString("#FF303030");
		public Color MaterialDesignCardBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF424242");
		public Color MaterialDesignToolBarBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF212121");
		public Color MaterialDesignBody { get; } = (Color)ColorConverter.ConvertFromString("#DDFFFFFF");
		public Color MaterialDesignBodyLight { get; } = (Color)ColorConverter.ConvertFromString("#89FFFFFF");
		public Color MaterialDesignColumnHeader { get; } = (Color)ColorConverter.ConvertFromString("#BCFFFFFF");
		public Color MaterialDesignCheckBoxOff { get; } = (Color)ColorConverter.ConvertFromString("#89FFFFFF");
		public Color MaterialDesignCheckBoxDisabled { get; } = (Color)ColorConverter.ConvertFromString("#FF647076");
		public Color MaterialDesignTextBoxBorder { get; } = (Color)ColorConverter.ConvertFromString("#89FFFFFF");
		public Color MaterialDesignDivider { get; } = (Color)ColorConverter.ConvertFromString("#1FFFFFFF");
		public Color MaterialDesignSelection { get; } = (Color)ColorConverter.ConvertFromString("#757575");
		public Color MaterialDesignToolForeground { get; } = (Color)ColorConverter.ConvertFromString("#FF616161");
		public Color MaterialDesignToolBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFe0e0e0");
		public Color MaterialDesignFlatButtonClick { get; } = (Color)ColorConverter.ConvertFromString("#19757575");
		public Color MaterialDesignFlatButtonRipple { get; } = (Color)ColorConverter.ConvertFromString("#FFB6B6B6");
		public Color MaterialDesignToolTipBackground { get; } = (Color)ColorConverter.ConvertFromString("#eeeeee");
		public Color MaterialDesignChipBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF2E3C43");
		public Color MaterialDesignSnackbarBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFCDCDCD");
		public Color MaterialDesignSnackbarMouseOver { get; } = (Color)ColorConverter.ConvertFromString("#FFB9B9BD");
		public Color MaterialDesignSnackbarRipple { get; } = (Color)ColorConverter.ConvertFromString("#FF494949");
		public Color MaterialDesignTextFieldBoxBackground { get; } = (Color)ColorConverter.ConvertFromString("#1AFFFFFF");
		public Color MaterialDesignTextFieldBoxHoverBackground { get; } = (Color)ColorConverter.ConvertFromString("#1FFFFFFF");
		public Color MaterialDesignTextFieldBoxDisabledBackground { get; } = (Color)ColorConverter.ConvertFromString("#0DFFFFFF");
		public Color MaterialDesignTextAreaBorder { get; } = (Color)ColorConverter.ConvertFromString("#BCFFFFFF");
		public Color MaterialDesignTextAreaInactiveBorder { get; } = (Color)ColorConverter.ConvertFromString("#29FFFFFF");
		public Color MaterialDesignDataGridRowHoverBackground { get; } = (Color)ColorConverter.ConvertFromString("#14FFFFFF");
		public Color PanelBackground { get; } = (Color)ColorConverter.ConvertFromString("#11FFFFFF");
	}

	public class LightTheme : IBaseTheme
	{
		public Color MaterialDesignValidationErrorColor { get; } = (Color)ColorConverter.ConvertFromString("#F44336");
		public Color MaterialDesignBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
		public Color MaterialDesignPaper { get; } = (Color)ColorConverter.ConvertFromString("#FFFAFAFA");
		public Color MaterialDesignCardBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
		public Color MaterialDesignToolBarBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFF5F5F5");
		public Color MaterialDesignBody { get; } = (Color)ColorConverter.ConvertFromString("#DD000000");
		public Color MaterialDesignBodyLight { get; } = (Color)ColorConverter.ConvertFromString("#DD000000");
		public Color MaterialDesignColumnHeader { get; } = (Color)ColorConverter.ConvertFromString("#BC000000");
		public Color MaterialDesignCheckBoxOff { get; } = (Color)ColorConverter.ConvertFromString("#89000000");
		public Color MaterialDesignCheckBoxDisabled { get; } = (Color)ColorConverter.ConvertFromString("#FFBDBDBD");
		public Color MaterialDesignTextBoxBorder { get; } = (Color)ColorConverter.ConvertFromString("#89000000");
		public Color MaterialDesignDivider { get; } = (Color)ColorConverter.ConvertFromString("#1F000000");
		public Color MaterialDesignSelection { get; } = (Color)ColorConverter.ConvertFromString("#FFDEDEDE");
		public Color MaterialDesignToolForeground { get; } = (Color)ColorConverter.ConvertFromString("#FF616161");
		public Color MaterialDesignToolBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFE0E0E0");
		public Color MaterialDesignFlatButtonClick { get; } = (Color)ColorConverter.ConvertFromString("#FFDEDEDE");
		public Color MaterialDesignFlatButtonRipple { get; } = (Color)ColorConverter.ConvertFromString("#FFB6B6B6");
		public Color MaterialDesignToolTipBackground { get; } = (Color)ColorConverter.ConvertFromString("#757575");
		public Color MaterialDesignChipBackground { get; } = (Color)ColorConverter.ConvertFromString("#12000000");
		public Color MaterialDesignSnackbarBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF323232");
		public Color MaterialDesignSnackbarMouseOver { get; } = (Color)ColorConverter.ConvertFromString("#FF464642");
		public Color MaterialDesignSnackbarRipple { get; } = (Color)ColorConverter.ConvertFromString("#FFB6B6B6");
		public Color MaterialDesignTextFieldBoxBackground { get; } = (Color)ColorConverter.ConvertFromString("#0F000000");
		public Color MaterialDesignTextFieldBoxHoverBackground { get; } = (Color)ColorConverter.ConvertFromString("#14000000");
		public Color MaterialDesignTextFieldBoxDisabledBackground { get; } = (Color)ColorConverter.ConvertFromString("#08000000");
		public Color MaterialDesignTextAreaBorder { get; } = (Color)ColorConverter.ConvertFromString("#BC000000");
		public Color MaterialDesignTextAreaInactiveBorder { get; } = (Color)ColorConverter.ConvertFromString("#29000000");
		public Color MaterialDesignDataGridRowHoverBackground { get; } = (Color)ColorConverter.ConvertFromString("#0A000000");
		public Color PanelBackground { get; } = (Color)ColorConverter.ConvertFromString("#55FFFFFF");
	}
}
