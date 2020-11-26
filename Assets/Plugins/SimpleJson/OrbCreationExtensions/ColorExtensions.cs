/* OrbCreationExtensions 1.0            */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* March 31, 2015                       */
/* games, components and freelance work */

using UnityEngine;
using System;
using System.Collections;

namespace OrbCreationExtensions
{
	public static class ColorExtensions
    {

		public static string MakeString(this Color aColor) {
			return MakeString(aColor, false);
		}
		public static string MakeString(this Color aColor, bool includeAlpha) {
			return MakeString((Color32)aColor, includeAlpha);
		}
		public static string MakeString(this Color32 aColor, bool includeAlpha) {
			string rs = Convert.ToString(aColor.r,16).ToUpper();
			string gs = Convert.ToString(aColor.g,16).ToUpper();
			string bs = Convert.ToString(aColor.b,16).ToUpper();
			string a_s = Convert.ToString(aColor.a,16).ToUpper();
			while(rs.Length < 2) rs= "0" + rs;
			while(gs.Length < 2) gs= "0" + gs;
			while(bs.Length < 2) bs= "0" + bs;
			while(a_s.Length < 2) a_s= "0" + a_s;
			if(includeAlpha) return "#"+ rs + gs + bs + a_s;
			return "#"+ rs + gs + bs;
		}
		public static Color MakeColor(this Color aValue) {
			return aValue;
		}
		public static Vector3 MakeHSB(this Color c) {
		    float minValue = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
		    float maxValue = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
		    float delta = maxValue - minValue;
		    float h = 0f;
		    float s = 0f;
		    float b = maxValue;

		    // Calc hue (in degrees between 0 and 360)
		    if(maxValue == c.r) {
		        if(c.g >= c.b) {
		            if(delta == 0f) h = 0f;
		            else h = 60f * (c.g - c.b) / delta;
		        } else if(c.g < c.b) {
		            h = 60f * (c.g - c.b) / delta + 360f;
		        }
		    } else if(maxValue == c.g) {
		        h = 60f * (c.b - c.r ) / delta + 120f;
		    } else if(maxValue == c.b) {
		        h = 60f * (c.r - c.g) / delta + 240f;
		    }

		    // Calc saturation (0 - 1)
		    if(maxValue == 0f) s = 0f;
		    else s = 1f - (minValue / maxValue);
		    return new Vector3(h / 360f, s, b);
		}

		public static float Hue(this Color c) {
			return MakeHSB(c).x;
		}
		public static float Saturation(this Color c) {
			return MakeHSB(c).y;
		}
		public static float Brightness(this Color c) {
			return MakeHSB(c).z;
		}

		public static float GetColorDistance(this Color color1, Color color2) {
			Vector3 v1 = new Vector3(color1.r, color1.g, color1.b);
			Vector3 v2 = new Vector3(color2.r, color2.g, color2.b);
			return Vector3.Distance(v1, v2);
		}
		public static float GrayScale(this Color color) {
			return (color.r + color.g + color.b) / 3f;
		}
	}
}
