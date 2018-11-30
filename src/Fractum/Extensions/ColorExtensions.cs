using System.Drawing;

namespace Fractum.Extensions
{
    internal static class ColorExtensions
    {
        public static int ToRGB(this Color color)
        {
            int rgb_int = color.R;
            rgb_int = (rgb_int << 8) + color.G;
            rgb_int = (rgb_int << 8) + color.B;

            return rgb_int;
        }

        public static Color FromRGB(this int rgb_int)
        {
            int r = (rgb_int >> 16) & 0xFF;
            int g = (rgb_int) >> 8 & 0xFF;
            int b = (rgb_int) & 0xFF;

            return Color.FromArgb(r, g, b);
        }
    }
}