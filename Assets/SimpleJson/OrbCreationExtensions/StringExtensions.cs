/* OrbCreationExtensions 1.0            */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* March 31, 2015                       */
/* games, components and freelance work */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace OrbCreationExtensions
{
    public static class StringExtensions
    {
        public static string MakeString(this string[] aPath) {
            string str="";
            if(aPath!=null) {
                if(aPath.Length>0) str=aPath[0];
                for(int i=1;i<aPath.Length;i++) {
                    if(aPath.Length<=0) return str;
                    str=str+"/"+aPath[i];
                }
            }
            return str; 
        }
        public static string[] TruncatePath(this string[] aPath,int newlen) {
            int i = 0;
            string[] newPath=new string[newlen];
            for(i=0;i<aPath.Length && i<newlen;i++) {
                newPath[i]=aPath[i];
            }
            return newPath;
        }

        public static string MakeString(this string aStr) {
            if(aStr==null) return "";
            return aStr;
        }
        public static int MakeInt(this string str) {
            int parsedInt = 0;
            if(str!=null && int.TryParse(str, out parsedInt)) return parsedInt;
            else return 0;
        }
        public static float MakeFloat(this string aStr) {
            float parsedFloat = 0.0f;
            if (aStr!=null && float.TryParse(aStr, out parsedFloat)) return parsedFloat;
            else return 0.0f;
        }
        public static double MakeDouble(this string aStr) {
            double parsed = 0.0;
            if (aStr!=null && double.TryParse(aStr, out parsed)) return parsed;
            else return 0.0;
        }
        public static bool MakeBool(this string aStr) {
            int parsedInt = 0;
            if(aStr.ToLower() == "true") return true;
            if(aStr!=null && int.TryParse(aStr, out parsedInt)) return (parsedInt > 0);
            else return false;
        }
        public static Color MakeColor(this string aStr) {
            Color clr = new Color(0,0,0,0);
            if(aStr!=null && aStr.Length>0) {
                try {
                    if(aStr.Substring(0,1)=="#") {  // #FFFFFF format
                        string str = aStr.Substring(1, aStr.Length - 1);
                        try {
                            clr.r = (float)System.Int32.Parse(str.Substring(0,2), NumberStyles.AllowHexSpecifier) / 255.0f;
                            clr.g = (float)System.Int32.Parse(str.Substring(2,2), NumberStyles.AllowHexSpecifier) / 255.0f;
                            clr.b = (float)System.Int32.Parse(str.Substring(4,2), NumberStyles.AllowHexSpecifier) / 255.0f;
                        } catch(Exception e) {
                            Debug.Log("Could not convert "+aStr+" to Color. "+e);
                        }
                        if(str.Length==8) clr.a = System.Int32.Parse(str.Substring(6,2), NumberStyles.AllowHexSpecifier) / 255.0f;
                        else clr.a = 1.0f;
                    } else if(aStr.IndexOf(",",0)>=0) {  // 0.3, 1.0, 0.2 format
                        int p0 = 0;
                        int p1 = 0;
                        int c = 0;
                        float divideBy = 1f;
                        if(aStr.IndexOf(".",0)<0) divideBy = 255f;
                        p1 = aStr.IndexOf(",",p0);
                        while(p1>p0 && c<4) {
                            clr[c++] = Mathf.Clamp01(aStr.Substring(p0,p1-p0).MakeFloat() / divideBy);
                            p0=p1+1;
                            if(p0 < aStr.Length) p1 = aStr.IndexOf(",",p0);
                            if(p1 < 0) p1 = aStr.Length;
                        }
                        if(c<4) clr.a = 1.0f;
                    } else if(aStr.IndexOf(" ",0)>=0) {  // 0.3 1.0 0.2 format
                        int p0 = 0;
                        int p1 = 0;
                        int c = 0;
                        float divideBy = 1f;
                        if(aStr.IndexOf(".",0)<0) divideBy = 255f;
                        p1 = aStr.IndexOf(" ",p0);
                        while(p1>p0 && c<4) {
                            clr[c++] = Mathf.Clamp01(aStr.Substring(p0,p1-p0).MakeFloat() / divideBy);
                            p0=p1+1;
                            if(p0 < aStr.Length) p1 = aStr.IndexOf(" ",p0);
                            if(p1 < 0) p1 = aStr.Length;
                        }
                        if(c<4) clr.a = 1.0f;
                    }
                } catch(Exception e) {
                    Debug.Log("Could not convert "+aStr+" to Color. "+e);
                }
            }
            return clr;
        }
        public static Vector3 MakeVector3(this string aStr) {
            Vector3 v = new Vector3(0,0,0);
            if(aStr!=null && aStr.Length>0) {
                try {
                    if(aStr.IndexOf(",",0)>=0) {  // 0.3, 1.0, 0.2 format
                        int p0 = 0;
                        int p1 = 0;
                        int c = 0;
                        p1 = aStr.IndexOf(",",p0);
                        while(p1>p0 && c<=3) {
                            v[c++] = float.Parse(aStr.Substring(p0,p1-p0));
                            p0=p1+1;
                            if(p0 < aStr.Length) p1 = aStr.IndexOf(",",p0);
                            if(p1 < 0) p1 = aStr.Length;
                        }
                    }
                } catch(Exception e) {
                    Debug.Log("Could not convert "+aStr+" to Vector3. "+e);
                    return new Vector3(0,0,0);
                }
            }
            return v;
        }
        public static Vector4 MakeVector4(this string aStr) {
            Vector4 v = new Vector4(0,0,0,0);
            if(aStr!=null && aStr.Length>0) {
                try {
                    if(aStr.IndexOf(",",0)>=0) {  // 0.3, 1.0, 0.2 format
                        int p0 = 0;
                        int p1 = 0;
                        int c = 0;
                        p1 = aStr.IndexOf(",",p0);
                        while(p1>p0 && c<=4) {
                            v[c++] = float.Parse(aStr.Substring(p0,p1-p0));
                            p0=p1+1;
                            if(p0 < aStr.Length) p1 = aStr.IndexOf(",",p0);
                            if(p1 < 0) p1 = aStr.Length;
                        }
                    }
                } catch(Exception e) {
                    Debug.Log("Could not convert "+aStr+" to Vector3. "+e);
                    return new Vector4(0,0,0,0);
                }
            }
            return v;
        }


        public static int IndexOfChars(this string str, string searchChars, int startAt) {
            for(int i=startAt;i<str.Length;i++) {
                char c = str[i];
                if(searchChars.IndexOf(c) >= 0) return i;
            }
            return -1;
        }
        public static int IndexOfEndOfLine(this string str, int startAt) {
            int i = str.IndexOf('\n', startAt);
            if(i >= startAt) {
                if(i>0 && str[i-1]=='\r') return i-1;                
                return i;
            }
            return str.IndexOf('\r', startAt);
        }
        public static int EndOfCharRepetition(this string str, int startAt) {
            if(startAt<str.Length) {
                int i = startAt;
                char c = str[i];
                while(i<str.Length-1) {
                    i++;
                    if(str[i]!=c) return i;
                }
            }
            return str.Length;
        }
        public static string Truncate(this string str, int maxLength) {
            if(str.Length>maxLength) return str.Substring(0,maxLength);
            return str;
        }
        public static string TrimChars(this string str, string trimChars) {
            int b = 0;
            int e = str.Length;
            for(;b<e;b++) {
                char c = str[b];
                if(trimChars.IndexOf(c) < 0) break;
            }

            for(;e>b;e--) {
                char c = str[e];
                if(trimChars.IndexOf(c) < 0) break;
            }
            if(b>0 || e<str.Length) return str.Substring(b,e-b);
            return str;
        }

        public static string SubstringAfter(this string str, string after) {
            int pos = str.IndexOf(after);
            if(pos>=0) {
                pos += after.Length;
                return str.Substring(pos, str.Length - pos);
            }
            return str;
        }
        public static List<int> ToIntList(this string str, char separator) {
            List<int> ints = new List<int>();
            int begin=0;
            bool digitPassed=false;
            str = str + separator;
            for(int i=0;i<str.Length;i++) {
                char c = str[i];
                if(c==separator || i==str.Length-1) {
                    if(begin<i && digitPassed) {
                        int parsedInt = 0;
                        if(int.TryParse(str.Substring(begin, i-begin), out parsedInt)) {
                            ints.Add(parsedInt);
                        }
                        digitPassed=false;
                    }
                    begin=i;
                }
                if((int)c>47 && (int)c<58) digitPassed=true;
                if(!digitPassed) begin=i;  // this skips all \t\r\n shit
            }
            return ints;
        }
        public static float[] ToFloatArray(this string str, char separator) {
            List<float> floats = new List<float>();
            IntoFloatList(str, ref floats, separator);
            return floats.ToArray();
        }
        public static List<float> ToFloatList(this string str, char separator) {
            List<float> floats = new List<float>();
            IntoFloatList(str, ref floats, separator);
            return floats;
        }
        public static void IntoFloatList(this string str, ref List<float> floats, char separator) {
            int begin=0;
            bool digitPassed=false;
            str = str + separator;
            for(int i=0;i<str.Length;i++) {
                char c = str[i];
                if(c==separator || c=='\n' || c=='\r' || i==str.Length-1) {
                    if(begin<i && digitPassed) {
                        float parsedFloat = 0.0f;
                        if(float.TryParse(str.Substring(begin, i-begin), out parsedFloat)) {
                            floats.Add(parsedFloat);
                        }
                        digitPassed=false;
                    }
                    begin=i;
                }
                if((int)c>47 && (int)c<58) digitPassed=true;
                if(!digitPassed) begin=i;  // this skips all \t\r\n shit
            }
        }

        public static List<Vector2> ToVector2List(this string str, char separator) {
            List<Vector2> vectors = new List<Vector2>();
            IntoVector2List(str, ref vectors, separator, 2);
            return vectors;
        }
        public static List<Vector2> ToVector2List(this string str, char separator, int floatsPerValue) {
            List<Vector2> vectors = new List<Vector2>();
            IntoVector2List(str, ref vectors, separator, floatsPerValue);
            return vectors;
        }
        public static void IntoVector2List(this string str, ref List<Vector2> vectors, char separator, int floatsPerValue) {
            int vectorIdx=0;
            Vector2 vector = new Vector2(0,0);
            int begin=0;
            bool digitPassed=false;
            str = str + separator;
            for(int i=0;i<str.Length;i++) {
                char c = str[i];
                if(c==separator || c=='\n' || c=='\r' || i==str.Length-1) {
                    if(begin<i && digitPassed) {
                        float parsedFloat = 0.0f;
                        if(float.TryParse(str.Substring(begin, i-begin), out parsedFloat)) {
                            if(vectorIdx<2) vector[vectorIdx] = parsedFloat;
                            vectorIdx++;
                            if(vectorIdx == floatsPerValue) {
                                vectorIdx = 0;
                                vectors.Add(vector);
                                vector = new Vector2(0,0);
                            }
                        }
                        digitPassed=false;
                    }
                    begin=i;
                }
                if((int)c>47 && (int)c<58) digitPassed=true;
                if(!digitPassed) begin=i;  // this skips all \t\r\n shit
            }
        }

        public static Vector3 ToVector3(this string str, char separator, Vector3 defaultValue) {
            List<Vector3> vectors = new List<Vector3>();
            IntoVector3List(str, ref vectors, separator);
            if(vectors.Count>0) return vectors[0];
            return defaultValue;
        }
        public static List<Vector3> ToVector3List(this string str, char separator) {
            List<Vector3> vectors = new List<Vector3>();
            IntoVector3List(str, ref vectors, separator);
            return vectors;
        }
        public static void IntoVector3List(this string str, ref List<Vector3> vectors, char separator) {
            int vectorIdx=0;
            Vector3 vector = new Vector3(0,0,0);
            int begin=0;
            bool digitPassed=false;
            str = str + separator;
            for(int i=0;i<str.Length;i++) {
                char c = str[i];
                if(c==separator || c=='\n' || c=='\r' || i==str.Length-1) {
                    if(begin<i && digitPassed) {
                        float parsedFloat = 0.0f;
                        if(float.TryParse(str.Substring(begin, i-begin), out parsedFloat)) {
                            vector[vectorIdx++] = parsedFloat;
                            if(vectorIdx == 3) {
                                vectorIdx = 0;
                                vectors.Add(vector);
                                vector = new Vector3(0,0,0);
                            }
                        }
                        digitPassed=false;
                    }
                    begin=i;
                }
                if((int)c>47 && (int)c<58) digitPassed=true;
                if(!digitPassed) begin=i;  // this skips all \t\r\n shit
            }
        }

        public static string FilterComments(this string str, string beginComment, string endComment, string replaceWith) {
            int begin = 0;
            int end = 0;
            begin = str.IndexOf(beginComment, end);
            while(begin > 0) {
                end = str.IndexOf(endComment, begin);
                if(end > begin) {
                    end += endComment.Length;
                    str = str.Substring(0,begin) + replaceWith + str.Substring(end, str.Length - end);
                } else break;
                begin = str.IndexOf(beginComment, begin);
            }
            return str;   
        }

        /* ------------------------------------------------------------------------------------- */
        /* -------------------- Simple string encoding for XML and JSON ------------------------ */

        // Since the name of the package is SimpleXml, this should suffice
        public static string XmlDecode(this string str) {
            str = str.Replace("&lt;","<");
            str = str.Replace("&gt;",">");
            str = str.Replace("&amp;","&");
            str = str.Replace("&apos;","'");
            str = str.Replace("&quot;","\"");
            return str;
        }
        public static string XmlEncode(this string str) {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("'", "&apos;");
            str = str.Replace("\"", "&quot;");
            return str;
        }

        public static string JsonDecode(this string str) {
            str = str.Replace("\\/", "/");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\r", "\r");
            str = str.Replace("\\t", "\t");
            str = str.Replace("\\\"", "\"");
            str = str.Replace("\\\\", "\\");
            return str;
        }
        public static string JsonEncode(this string str) {
            str = str.Replace("\"", "\\\"");
            str = str.Replace("\\", "\\\\");
//          str = str.Replace("/", "\\/");
            str = str.Replace("\n", "\\n");
            str = str.Replace("\r", "\\r");
            str = str.Replace("\t", "\\t");
            return str;
        }
        /* ------------------------------------------------------------------------------------- */

    }
}