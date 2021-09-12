using System;
using System.Globalization;
using System.Windows.Forms;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	internal static class ScreenExtensions
	{
		public static double ParseSize(this Screen screen, string value, Direction direction)
		{
			if (screen == null)
			{
				throw new ArgumentNullException(nameof(screen));
			}
			else if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value.EndsWith("%", StringComparison.Ordinal))
			{
				var percentage = double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture)/100.0;
				return direction == Direction.Horizontal ? percentage * screen.Bounds.Width : percentage * screen.Bounds.Height;
			}
			else
			{
				return double.Parse(value, CultureInfo.InvariantCulture);
			}
		}
	}

	internal enum Direction
	{
		Horizontal,
		Vertical
	}
}
