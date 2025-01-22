// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using FontAwesome.Sharp;
using System.Windows.Media;

public class IconBlock : IconBlockBase<IconChar>
{
	private static readonly Typeface[] Typefaces = typeof(IconHelper).Assembly.LoadTypefaces("fonts", new[] { "Font Awesome 6 Free Solid" });

	public IconBlock()
		: base(Font)
	{
	}

	private static FontFamily Font => Typefaces[0].FontFamily;

	protected override FontFamily FontFor(IconChar icon)
	{
		return Font;
	}
}
