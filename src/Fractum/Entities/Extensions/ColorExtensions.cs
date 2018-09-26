using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Fractum.Rest.Extensions
{
    public static class ColorExtensions
    {
        public static int ToRGB(this Color color)
            => (color.R * 256 * 256) + (color.G * 256) + color.B;
    }
}
