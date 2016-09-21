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
    public static class FloatExtensions
    {
		public static string MakeString(this float aFloat) {
			return ""+aFloat;
		}
		public static string MakeString(this float aFloat, int decimals) {
			if(decimals<=0) return ""+Mathf.RoundToInt(aFloat);
			string format = "{0:F"+decimals+"}";
			return string.Format(format, aFloat);
		}
		public static int MakeInt(this float aFloat) {
			return Mathf.FloorToInt((float)aFloat);
		}
		public static bool MakeBool(this float aFloat) {
			return aFloat>0f;
		}
		public static float MakeFloat(this float aFloat) {
			return aFloat;
		}
		public static double MakeDouble(this float aFloat) {
			return (double)aFloat;
		}

		public static string MakeString(this double aDouble) {
			return ""+aDouble;
		}
		public static string MakeString(this double aDouble, int decimals) {
			if(decimals<=0) {
				int v = (int)aDouble;
				if(v>=0 && (aDouble-v)>=0.5) v++;
				if(v<0 && (aDouble-v)<=-0.5) v--;
				return ""+v;
			}
			string format = "{0:F"+decimals+"}";
			return string.Format(format, aDouble);
		}
		public static int MakeInt(this double aDouble) {
			return (int)aDouble;
		}
		public static bool MakeBool(this double aDouble) {
			return aDouble>0.0;
		}
		public static float MakeFloat(this double aDouble) {
			return (float)aDouble;
		}
		public static double MakeDouble(this double aDouble) {
			return aDouble;
		}


		public static float To180Angle(this float f) {
			while(f<=-180.0f) f+=360.0f;
			while(f>180.0f) f-=360.0f;
			return f;
		}

		public static float To360Angle(this float f) {
			while(f<0.0f) f+=360.0f;
			while(f>=360.0f) f-=360.0f;
			return f;
		}

		public static float RadToCompassAngle(this float rad) {
			return DegreesToCompassAngle(rad * Mathf.Rad2Deg);
		}
		public static float DegreesToCompassAngle(this float angle) {
			angle = 90.0f - angle;
			return To360Angle(angle);
		}

		public static float CompassAngleLerp(this float from, float to, float portion) {
			float dif = (to-from).To180Angle();
			dif *= Mathf.Clamp01(portion);
			return (from+dif).To360Angle();
		}

		public static float RelativePositionBetweenAngles(this float angle, float from, float to) {
			from = from.To360Angle();
			to = to.To360Angle();
			if((from - to) > 180f) from = from - 360f;
			if((to - from) > 180f) to = to - 360f;
			angle = angle.To360Angle();
			if(from < to) {
				if(angle >= from && angle < to) return (angle - from) / (to - from);
				if(angle - 360f >= from && angle - 360f < to) return (angle - 360f - from) / (to - from);
			}
			if(from > to) {
				if(angle < from && angle >= to) return (angle - to) / (from - to);
				if(angle - 360f < from && angle - 360f >= to) return (angle - 360f - to) / (from - to);
			}
			return -1f;
		}
		public static float Distance(this float f1, float f2) {
			return Mathf.Abs(f1 - f2);
		}
		public static float Round(this float f, int decimals) {
			float multiplier = Mathf.Pow(10, decimals);
			f = Mathf.Round(f * multiplier);
			return f / multiplier;
		}
		public static float Cube(this float f) {
			return f * f;
		}
	}
}
