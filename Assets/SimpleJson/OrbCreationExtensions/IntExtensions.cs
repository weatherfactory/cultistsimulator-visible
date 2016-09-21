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
    public static class IntExtensions
    {
        public static string MakeString(this int[] anArray) {
            string str="";
            if(anArray!=null) {
                if(anArray.Length>0) str=str+anArray[0];
                for(int i=1;i<anArray.Length;i++) {
                    if(anArray.Length<=0) return str;
                    str=str+","+anArray[i];
                }
            }
            return str; 
        }
        public static bool ContentsEqualTo(this int[] anArray, int[] array2) {
        	for(int i=0;i<anArray.Length;i++) {
        		if(array2.Length<=i || anArray[i]!=array2[i]) return false;
        	}
        	return true;
        }


		public static string MakeString(this int anInt) {
			return ""+anInt;
		}
		public static int MakeInt(this int anInt) {
			return anInt;
		}
		public static bool MakeBool(this int anInt) {
			return anInt>0f;
		}
		public static float MakeFloat(this int anInt) {
			return (float)anInt;
		}
		public static double MakeDouble(this int anInt) {
			return (double)anInt;
		}


	}

    public static class BoolExtensions
    {
		public static string MakeString(this bool aBool) {
			if(aBool) return "1";
			return "0";
		}
		public static int MakeInt(this bool aBool) {
			if(aBool) return 1;
			return 0;
		}
		public static bool MakeBool(this bool aBool) {
			return aBool;
		}
		public static float MakeFloat(this bool aBool) {
			if(aBool) return 1f;
			return 0f;
		}
		public static double MakeDouble(this bool aBool) {
			if(aBool) return 1.0;
			return 0.0;
		}


	}

    public static class ObjectExtensions
    {
		public static string MakeString(this object anObject) {
			if(anObject==null) return "";
			else if(anObject.GetType()==typeof(string)) return (string)anObject;
			else if(anObject.GetType()==typeof(bool)) return ((bool)anObject).MakeString();
			else if(anObject.GetType()==typeof(int)) return ((int)anObject).MakeString();
			else if(anObject.GetType()==typeof(float)) return ((float)anObject).MakeString();
			else if(anObject.GetType()==typeof(double)) return ((double)anObject).MakeString();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeString();
			return ""+anObject;
		}

		public static int MakeInt(this object anObject) {
			if(anObject==null) return 0;
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeInt();
			else if(anObject.GetType()==typeof(bool)) return ((bool)anObject).MakeInt();
			else if(anObject.GetType()==typeof(int)) return ((int)anObject).MakeInt();
			else if(anObject.GetType()==typeof(float)) return ((float)anObject).MakeInt();
			else if(anObject.GetType()==typeof(double)) return ((double)anObject).MakeInt();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeInt();
			return 0;
		}

		public static bool MakeBool(this object anObject) {
			if(anObject==null) return false;
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeBool();
			else if(anObject.GetType()==typeof(bool)) return ((bool)anObject).MakeBool();
			else if(anObject.GetType()==typeof(int)) return ((int)anObject).MakeBool();
			else if(anObject.GetType()==typeof(float)) return ((float)anObject).MakeBool();
			else if(anObject.GetType()==typeof(double)) return ((double)anObject).MakeBool();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeBool();
			return true;
		}

		public static float MakeFloat(this object anObject) {
			if(anObject==null) return 0f;
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeFloat();
			else if(anObject.GetType()==typeof(bool)) return ((bool)anObject).MakeFloat();
			else if(anObject.GetType()==typeof(int)) return ((int)anObject).MakeFloat();
			else if(anObject.GetType()==typeof(float)) return ((float)anObject).MakeFloat();
			else if(anObject.GetType()==typeof(double)) return ((double)anObject).MakeFloat();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeFloat();
			return 0f;
		}

		public static double MakeDouble(this object anObject) {
			if(anObject==null) return 0.0;
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeDouble();
			else if(anObject.GetType()==typeof(bool)) return ((bool)anObject).MakeDouble();
			else if(anObject.GetType()==typeof(int)) return ((int)anObject).MakeDouble();
			else if(anObject.GetType()==typeof(float)) return ((float)anObject).MakeDouble();
			else if(anObject.GetType()==typeof(double)) return ((double)anObject).MakeDouble();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeDouble();
			return 0.0;
		}

		public static Color MakeColor(this object anObject) {
			if(anObject==null) new Color(0,0,0,0);
			else if(anObject.GetType()==typeof(Color)) return ((Color)anObject).MakeColor();
			else if(anObject.GetType()==typeof(string)) return ((string)anObject).MakeColor();
			else if(anObject.GetType()==typeof(Vector3)) return ((Vector3)anObject).MakeColor();
			else if(anObject.GetType()==typeof(Vector4)) return ((Vector4)anObject).MakeColor();
			return new Color(0,0,0,0);
		}


	}

}
