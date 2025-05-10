// © XIV-Tools.
// Licensed under the MIT license.

[assembly: System.Runtime.CompilerServices.DisableRuntimeMarshalling]

namespace XivToolsWpf.Utility;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static partial class Win32
{
	private const int WM_NCPAINT = 0x0085;
	private const int WM_NCACTIVATE = 0x0086;

	public static readonly IntPtr InvisibleRegion = CreateRectRgn(0, 0, -1, -1);

	public enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
		ACCENT_INVALID_STATE = 5,
	}

	/// <summary>
	/// Options used by the WINDOWCOMPOSITIONATTRIBDATA structure.
	/// Used with the GetWindowCompositionAttribute and SetWindowCompositionAttribute functions.
	/// </summary>
	/// <remarks>
	/// Source: https://learn.microsoft.com/en-us/windows/win32/dwm/windowcompositionattribdata
	/// </remarks>
	public enum WindowCompositionAttribute
	{
		WCA_ACCENT_POLICY = 19,
		WCA_USEDARKMODECOLORS = 26,
	}

	/// <summary>
	/// Flags for the DWM_BLURBEHIND structure.
	/// </summary>
	/// <remarks>
	/// Source: https://learn.microsoft.com/en-us/windows/win32/dwm/dwm-bb-constants
	/// </remarks>
	public enum DwmBlurBehindFlags : int
	{
		DWM_BB_ENABLE = 0x00000001,
		DWM_BB_BLURREGION = 0x00000002,
		DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004,
	}

	/// <summary>
	/// Flags for specyfing the system-drawn backdrop material of a window, including the non-client area.
	/// </summary>
	/// <remarks>
	/// Source: https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type
	/// </remarks>
	public enum DwmSystemBackdropType : uint
	{
		/// <summary>Automatic (Default)</summary>
		DWMSBT_AUTO,

		/// <summary>No Backdrop</summary>
		DWMSBT_NONE,

		/// <summary>Mica</summary>
		DWMSBT_MAINWINDOW,

		/// <summary>Acrylic</summary>
		DWMSBT_TRANSIENTWINDOW,

		/// <summary>Mica Alt</summary>
		DWMSBT_TABBEDWINDOW,
	}

	/// <summary>
	/// Options used by the DwmSetWindowAttribute and DwmSetWindowAttribute functions.
	/// </summary>
	/// <remarks>
	/// Source: https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute
	/// </remarks>
	public enum DwmWindowAttribute : int
	{
		/// <summary>
		/// Specifies the colorization color of the window frame.
		/// </summary>
		/// <remarks>
		/// For versions prior to Windows 10 20H1 (Build 19041).
		/// </remarks>
		DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,

		/// <summary>
		/// Specifies the colorization color of the window frame.
		/// Whenever set to true, dark mode will be honored. Otherwise, light mode will be used.
		/// </summary>
		/// <remarks>
		/// Supported starting Windows 11 Build 22000.
		/// </remarks>
		DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

		/// <summary>
		/// Specifies the color of the window caption (i.e., the title bar)
		/// </summary>
		/// <remarks>
		/// Supported starting Windows 11 Build 22000.
		/// </remarks>
		DWMWA_CAPTION_COLOR = 35,

		/// <summary>
		/// Specifies the system-drawn backdrop material of a window, including the non-client area.
		/// </summary>
		/// <remarks>
		/// Supported starting Windows 11 Build 22621.
		/// </remarks>
		DWMWA_SYSTEMBACKDROP_TYPE = 38,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AccentPolicy
	{
		public AccentState AccentState;
		public uint AccentFlags;
		public uint GradientColor;
		public uint AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DWM_BLURBEHIND
	{
		public int Flags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public IntPtr RgnBlur;

		[MarshalAs(UnmanagedType.Bool)]
		public bool TransitionOnMaximized;
	}

	public static bool IsWindows11() => Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 22000;
	public static bool IsWindows10() => Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build < 22000;

	public static void SetWindowDarkMode(nint windowHandle, bool darkMode)
	{
		bool isWindows11 = IsWindows11();
		bool isWindows10 = IsWindows10();

		if (isWindows11)
		{
			// Set titlebar color (Windows 11 Build 22000 and higher)
			// Note: This affects the caption control button foreground as well.
			uint colorPvAttribute = darkMode ? (uint)0x303030 : (uint)0xFAFAFA;
			_ = DwmSetWindowAttribute(windowHandle, (int)DwmWindowAttribute.DWMWA_CAPTION_COLOR, ref colorPvAttribute, sizeof(uint));
		}

		if (isWindows10 || isWindows11)
		{
			int dwmAttribute = (int)DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE;
			if (Environment.OSVersion.Version.Build < 19041)
				dwmAttribute = (int)DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

			// Set immersive dark mode
			uint useImmersiveDarkMode = darkMode ? 1u : 0u;
			_ = DwmSetWindowAttribute(windowHandle, dwmAttribute, ref useImmersiveDarkMode, sizeof(uint));
		}

		unsafe
		{
			int darkModeAttribute = darkMode ? 1 : 0;
			WindowCompositionAttributeData data = new()
			{
				Attribute = WindowCompositionAttribute.WCA_USEDARKMODECOLORS,
				SizeOfData = sizeof(int),
				Data = (IntPtr)(&darkModeAttribute),
			};

			_ = SetWindowCompositionAttribute(windowHandle, ref data);
		}
	}

	public static void SetBlurBehindWindow(IntPtr hwnd, bool enable)
	{
		DWM_BLURBEHIND blurBehind = new()
		{
			Flags = (int)DwmBlurBehindFlags.DWM_BB_ENABLE | (int)DwmBlurBehindFlags.DWM_BB_BLURREGION,
			Enable = enable,
			RgnBlur = enable ? InvisibleRegion : IntPtr.Zero,
		};

		DwmEnableBlurBehindWindow(hwnd, ref blurBehind);
	}

	public static void SetAccentPolicy(IntPtr hwnd, AccentPolicy accentPolicy)
	{
		unsafe
		{
			WindowCompositionAttributeData data = new()
			{
				Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
				SizeOfData = sizeof(AccentPolicy),
				Data = (IntPtr)(&accentPolicy),
			};

			_ = SetWindowCompositionAttribute(hwnd, ref data);
		}
	}

	public static void SetSystemBackdropType(IntPtr hwnd, DwmSystemBackdropType backdropType)
	{
		if (Environment.OSVersion.Version.Build >= 22621)
		{
			uint backdropTypeValue = (uint)backdropType;
			_ = DwmSetWindowAttribute(hwnd, (int)DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropTypeValue, sizeof(uint));
		}
	}

	public static void RedrawNonClientArea(Window window)
	{
		var hwnd = new WindowInteropHelper(window).Handle;
		_ = SendMessage(hwnd, WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
		_ = SendMessage(hwnd, WM_NCACTIVATE, window.IsActive ? 1 : 0, IntPtr.Zero);
	}

	[LibraryImport("user32.dll")]
	private static partial int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

	[LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
	private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[LibraryImport("dwmapi.dll")]
	private static partial void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

	[LibraryImport("gdi32.dll")]
	private static partial IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

	[LibraryImport("dwmapi.dll")]
	private static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref uint pvAttribute, int cbAttribute);
}
