using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace naid {
    public class ColorTools {
        /// <summary>
        /// Returns a color that is a little darker than srcColor.
        /// </summary>
        public static Color DarkerColor(Color srcColor) {
            const int AMOUNT = 10;
            return Color.FromArgb(srcColor.A, 
                Math.Max(0, srcColor.R - AMOUNT), 
                Math.Max(0, srcColor.G - AMOUNT),
                Math.Max(0, srcColor.B - AMOUNT)
            );

        }


        /// <summary>
        /// Parses a #rrggbb string which is used to specify color in HTML and CSS.
        /// </summary>
        public static Color ColorFromHTML(string HTMLcolor) {
            string r_part = HTMLcolor.Substring(1, 2);
            string g_part = HTMLcolor.Substring(3, 2);
            string b_part = HTMLcolor.Substring(5, 2);

            int r = int.Parse(r_part, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(g_part, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(b_part, System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(r, g, b);
        }
    }
}
