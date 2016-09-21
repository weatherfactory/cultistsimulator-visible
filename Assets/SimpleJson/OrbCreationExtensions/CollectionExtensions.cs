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

namespace OrbCreationExtensions
{
    public static class CollectionExtensions
    {
        public static object GetValue(this Hashtable hash, object key) {
            if(hash.ContainsKey(key)) return hash[key];
            return null;
        }
        public static Hashtable GetHashtable(this Hashtable hash, object key) {
            if(hash.ContainsKey(key) && hash[key].GetType() == typeof(Hashtable)) return (Hashtable)hash[key];
            return null;
        }
        public static ArrayList GetArrayList(this Hashtable hash, object key) {
            if(hash.ContainsKey(key) && hash[key].GetType() == typeof(ArrayList)) return (ArrayList)hash[key];
            return null;
        }
        public static ArrayList GetArrayList(this Hashtable hash, object key, bool wrap) {
            if(hash.ContainsKey(key)) {
                if(hash[key].GetType() == typeof(ArrayList)) return (ArrayList)hash[key];
                else if(wrap) {
                    ArrayList wrapper = new ArrayList();
                    wrapper.Add(hash[key]);
                    return wrapper;
                }
            }
            return null;
        }

        public static string GetString(this Hashtable hash, object key) {
            return GetString(hash, key, null);
        }
        public static string GetString(this Hashtable hash, object key, string defaultValue) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return defaultValue;
                if(value.GetType() == typeof(string)) return (string)value;
                if(value.GetType() == typeof(bool)) return ((bool)value).MakeString();
                if(value.GetType() == typeof(int) || value.GetType() == typeof(long)) return ((int)value).MakeString();
                if(value.GetType() == typeof(float) || value.GetType() == typeof(double)) return ((float)value).MakeString();
            }
            return defaultValue;
        }
        public static int GetInt(this Hashtable hash, object key) {
            return GetInt(hash, key, 0);
        }
        public static int GetInt(this Hashtable hash, object key, int defaultValue) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return defaultValue;
                if(value.GetType() == typeof(string)) return ((string)value).MakeInt();
                if(value.GetType() == typeof(bool) && (bool)value) return 1;
                if(value.GetType() == typeof(int) || value.GetType() == typeof(long)) return (int)value;
                if(value.GetType() == typeof(float) || value.GetType() == typeof(double)) return Mathf.FloorToInt((float)value);
            }
            return defaultValue;
        }
        public static float GetFloat(this Hashtable hash, object key) {
            return GetFloat(hash, key, 0f);
        }
        public static float GetFloat(this Hashtable hash, object key, float defaultValue) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return defaultValue;
                if(value.GetType() == typeof(string)) return ((string)value).MakeFloat();
                if(value.GetType() == typeof(bool) && (bool)value) return 1f;
                if(value.GetType() == typeof(int) || value.GetType() == typeof(long)) return (float)value;
                if(value.GetType() == typeof(float) || value.GetType() == typeof(double)) return (float)value;
            }
            return defaultValue;
        }
        public static bool GetBool(this Hashtable hash, object key) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return false;
                if(value.GetType() == typeof(string)) return ((string)value).MakeBool();
                if(value.GetType() == typeof(bool) && (bool)value) return (bool)value;
                if(value.GetType() == typeof(int) || value.GetType() == typeof(long)) return (int)value>0;
                if(value.GetType() == typeof(float) || value.GetType() == typeof(double)) return (float)value>0f;
            }
            return false;
        }
        public static Color GetColor(this Hashtable hash, object key) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return new Color(0,0,0,0);
                if(value.GetType() == typeof(string)) return ((string)value).MakeColor();
                if(value.GetType() == typeof(Color)) return (Color)value;
            }
            return new Color(0,0,0,0);
        }
        public static Vector3 GetVector3(this Hashtable hash, object key) {
            object value = null;
            if(hash.ContainsKey(key)) {
                value = hash[key];
                if(value == null) return Vector3.zero;
                if(value.GetType() == typeof(string)) return ((string)value).MakeVector3();
                if(value.GetType() == typeof(Vector3)) return (Vector3)value;
            }
            return Vector3.zero;
        }

        public static Texture2D GetTexture2D(this Hashtable hash, object key) {
            if(hash.ContainsKey(key)) {
                object value = hash[key];
                if(value == null) return null;
                if(value.GetType() == typeof(Texture2D)) return (Texture2D)value;
            }
            return null;
        }
        public static byte[] GetBytes(this Hashtable hash, object key) {
            if(hash.ContainsKey(key)) {
                object value = hash[key];
                if(value == null) return null;
                if(value.GetType() == typeof(byte[])) return (byte[])value;
            }
            return null;
        }

        public static void AddHashtable(this Hashtable hash, Hashtable addHash, bool overwriteExistingKeys) {
            ICollection keyColl = addHash.Keys;
            foreach(string key in keyColl) {
                if(overwriteExistingKeys || (!hash.ContainsKey(key))) hash[key] = addHash[key];
            }
        }

        /* ------------------------------------------------------------------------------------- */
        /* ------------------------------- Convert to XML string ------------------------------- */

        public static string XmlString(this Hashtable hash) {
            string str = "";
            XmlString(hash, ref str, 0);   // call the private method
            return str;
        }
        public static string XmlString(this ArrayList arr) {
            string str = "";
            XmlString(arr, ref str, 0);   // call the private method
            return str;
        }

        private static void XmlString(this Hashtable hash, ref string str, int level) {
            bool includePropertiesInTag = false;
            string tagName = null;

            // if the hashtable contains the special key ".tag.", we add an extra level
            if(hash.ContainsKey(".tag.")) {
                tagName = (string)hash[".tag."];
                MoveToNewLineIfNeeded(ref str, level);
                str = str + "<" + tagName;
                if(hash.Count<6 && hash.ContainsKey(tagName)) {
                    includePropertiesInTag = true;
                    ICollection keyColl = hash.Keys;
                    foreach(string key in keyColl) {
                        if(key!=tagName && (
                            hash[key].GetType() == typeof(Hashtable) || 
                            hash[key].GetType() == typeof(ArrayList))) {
                            includePropertiesInTag = false;
                        }
                    }
                }
                if(includePropertiesInTag) {
                    ICollection keyColl = hash.Keys;
                    foreach(string key in keyColl) {
                        if(key!=tagName && key!=".tag.") {
                            str = str + " " + key + "=\"" + hash[key] + "\"";
                        }
                    }       
                }
                str = str + ">\n";
                level++;
            }
            if(includePropertiesInTag) {
                MoveToNewLineIfNeeded(ref str, level);
                str = str + hash[tagName]+"\n";
            } else {
                // add all Hashtable keys to the export string
                ICollection keyColl = hash.Keys;
                foreach(string key in keyColl) {
                    if(key != ".tag.") {
                        object value = hash[key];
                        MoveToNewLineIfNeeded(ref str, level);
                        str = str + "<" + key + ">";
                        level++;

                        // since we dont know the type of the property, we need to test it
                        if(value==null) {
                            str = str + "NULL";
                        } else if(value.GetType() == typeof(Hashtable)) {
                            XmlString((Hashtable)value, ref str, level);
                        } else if(value.GetType() == typeof(ArrayList)) {
                            XmlString((ArrayList)value, ref str, level);
                        } else if(value.GetType() == typeof(string)) {
                            str = str + ((string)value).XmlEncode();  // encode the string to prevent illegal chracters
                        } else {
                            str = str + value;
                        }
                        level--;
                        MoveToNewLineIfNeeded(ref str, level);
                        str = str + "</" + key + ">\n";
                    }
                }
            }

            // if the hashtable contains the special key ".tag.", close off the extra level
            if(hash.ContainsKey(".tag.")) {
                level--;
                MoveToNewLineIfNeeded(ref str, level);
                str = str + "</" + tagName + ">\n";
            }
        }

        private static void XmlString(this ArrayList arr, ref string str, int level) {
//          level++;
            MoveToNewLineIfNeeded(ref str, level);

            // loop through the array elements
            for(int a=0;a<arr.Count;a++) {
                object value = arr[a];
                MoveToNewLineIfNeeded(ref str, level);

                // since we dont know the type of the element, we need to test it
                if(value==null) {
                    str = str + "NULL";
                    str = str + "\n";
                } else if(value.GetType() == typeof(Hashtable)) {
                    XmlString((Hashtable)value, ref str, level);
                } else if(value.GetType() == typeof(ArrayList)) {
                    XmlString((ArrayList)value, ref str, level);
                } else if(value.GetType() == typeof(string)) {
                    str = str + ((string)value).XmlEncode();  // encode the string to prevent illegal chracters
                    str = str + "\n";
                } else {
                    str = str + value;
                    str = str + "\n";
                }
            }
//          level--;
        }
        private static void MoveToNewLineIfNeeded(ref string str, int level) {
            // test the last char to see if we need to add a newline and a load of tabs
            // or just a load of tabs
            if(str.Length>0 && str.Substring(str.Length-1)==">") {
                str = str + "\n";
                for(int i=0;i<level;i++) str=str+"\t";
            } else if(str.Length>0 && str.Substring(str.Length-1)=="\n") {
                for(int i=0;i<level;i++) str=str+"\t";
            }
        }
        /* ------------------------------------------------------------------------------------- */

        /* ------------------------------------------------------------------------------------- */
        /* ------------------------------- Convert to JSON string ------------------------------ */

        public static string JsonString(this Hashtable hash) {
            string str = "";
            JsonString(hash, ref str, 0, true);  // call the private method
            return str;
        }
        public static string JsonString(this ArrayList arr) {
            string str = "";
            JsonString(arr, ref str, 0, true);  // call the private method
            return str;
        }
        public static string JsonString(this Hashtable hash, bool encode) {
            string str = "";
            JsonString(hash, ref str, 0, encode);  // call the private method
            return str;
        }
        public static string JsonString(this ArrayList arr, bool encode) {
            string str = "";
            JsonString(arr, ref str, 0, encode);  // call the private method
            return str;
        }


        private static void JsonString(this Hashtable hash, ref string str, int level, bool encode) {
            str=str+"{\n";
            level++;

            // loop through all the keys
            int count = 0;
            ICollection keyColl = hash.Keys;
            foreach(string key in keyColl){
                object value = hash[key];
                for(int i=0;i<level;i++) str=str+"\t";
                if(encode) str = str + "\"" + key + "\"" +": ";
                else str = str + key + ": ";

                // since we dont know the type of the property, we need to test it
                if(value==null) {
                    str = str + "NULL";
                } else if(value.GetType() == typeof(Hashtable)) {
                    JsonString((Hashtable)value, ref str, level, encode);
                } else if(value.GetType() == typeof(ArrayList)) {
                    JsonString((ArrayList)value, ref str, level, encode);
                } else if(value.GetType() == typeof(string)) {
                    if(encode) str = str + "\"" + ((string)value).JsonEncode() + "\"";  // encode the string to prevent illegal chracters
                    else str = str + (string)value; 
                } else {
                    str = str + "\"" + value + "\"";
                }
                count++;
                if(count < hash.Count) str = str + ",";
                str = str + "\n";
            }
            level--;
            for(int i=0;i<level;i++) str=str+"\t";
            str = str + "}";
        }

        private static void JsonString(this ArrayList arr, ref string str, int level, bool encode) {
            str=str+"[\n";
            level++;
            // loop through all the elements
            for(int a=0;a<arr.Count;a++) {
                object value = arr[a];
                for(int i=0;i<level;i++) str=str+"\t";

                // since we dont know the type of the element, we need to test it
                if(value==null) {
                    str = str + "NULL";
                } else if(value.GetType() == typeof(Hashtable)) {
                    JsonString((Hashtable)value, ref str, level, encode);
                } else if(value.GetType() == typeof(ArrayList)) {
                    JsonString((ArrayList)value, ref str, level, encode);
                } else if(value.GetType() == typeof(string)) {
                    if(encode) str = str + "\"" + ((string)value).JsonEncode() + "\"";  // encode the string to prevent illegal chracters
                    else str = str + (string)value; 
                } else {
                    str = str + "\"" + value + "\"";
                }
                if(a < arr.Count-1) str = str + ",";
                str = str + "\n";
            }
            level--;
            for(int i=0;i<level;i++) str=str+"\t";
            str = str + "]";
        }
        /* ------------------------------------------------------------------------------------- */

        public static object GetNodeAtPath(this ArrayList inNodeList, string[] path) {
            return GetNodeAtPath(inNodeList, path, 0);  // call the private method
        }
        public static object GetNodeAtPath(this Hashtable inNodeHash, string[] path) {
            return GetNodeAtPath(inNodeHash, path, 0);  // call the private method
        }

        private static object GetNodeAtPath(ArrayList inNodeList, string[] path, int level) {
            if(inNodeList == null) return null;
            string aTag = path[level];   // get path entry for current level

            // loop through the array elements
            for(int j=0;j<inNodeList.Count;j++) {
                object node = inNodeList[j];
                if(node!=null) {
                    if(node.GetType() == typeof(Hashtable)) {
                        if(((Hashtable)node).ContainsKey(".tag.")) {
                            // see if the element contains the special key ".tag." and test if it equals the path
                            if((string)((Hashtable)node)[".tag."] == aTag) {
                                if(level == path.Length-1) {  // this is the last entry in path
                                    return node;  // found our guy
                                } else {
                                    // pass the search on and move down a level in path
                                    return GetNodeAtPath((Hashtable)node, path, level+1);
                                }
                            }
                        } else {
                            // pass the search on to the deeper element
                            return GetNodeAtPath((Hashtable)node, path, level);
                        }
                    } else if(node.GetType() == typeof(ArrayList)) {
                        // pass the search on to the deeper element
                        return GetNodeAtPath((ArrayList)node, path, level);
                    }
                }
            }
            return null;
        }
        private static object GetNodeAtPath(Hashtable inNodeHash, string[] path, int level) {
            if(inNodeHash == null) return null;
            string aTag = path[level];   // get path entry for current level

            //see if our Hashtable contains the path entry
            if(inNodeHash.ContainsKey(aTag)) {
                object node = inNodeHash[aTag];

                if(level == path.Length-1) {  // this is the last entry in path
                    return inNodeHash[aTag];  // found our guy
                } else if(node!=null) {
                    // pass the search on and move down a level in path
                    if(node.GetType() == typeof(Hashtable)) return GetNodeAtPath((Hashtable)node, path, level+1);
                    else if(node.GetType() == typeof(ArrayList)) return GetNodeAtPath((ArrayList)node, path, level+1);
                }
            }
            return null;
        }

        public static Hashtable GetNodeWithProperty(this ArrayList inNodeList, string aKey, string aValue) {
            if(inNodeList == null) return null;

            // loop through the array elements
            for(int j=0;j<inNodeList.Count;j++) {
                object node = inNodeList[j];
                if(node!=null) {
                    Hashtable result = null;
                    // since ArrayLists do not have keys, we can only pass the search on to the elements
                    if(node.GetType() == typeof(Hashtable)) result = GetNodeWithProperty((Hashtable)node, aKey, aValue);
                    else if(node.GetType() == typeof(ArrayList)) result = GetNodeWithProperty((ArrayList)node, aKey, aValue);
                    if(result!=null) return result;
                }
            }
            return null;
        }
        public static Hashtable GetNodeWithProperty(this Hashtable inNodeHash, string aKey, string aValue) {
            if(inNodeHash == null) return null;

            // loop through all the keys
            ICollection keyColl = inNodeHash.Keys;
            foreach(string key in keyColl){
                object node = inNodeHash[key];
                if(node!=null) {
                    Hashtable result = null;
                    if(key == aKey) {  // found the key, now test the value
                        string nodeValue = ""+node;   // convert nodeValue to string
                        if(nodeValue == aValue) result = inNodeHash;  // found it
                    } else if(node.GetType() == typeof(Hashtable)) {
                        // pass the search on
                        result = GetNodeWithProperty((Hashtable)node, aKey, aValue);
                    } else if(node.GetType() == typeof(ArrayList)) {
                        // pass the search on
                        result = GetNodeWithProperty((ArrayList)node, aKey, aValue);
                    }
                    if(result!=null) return result;
                }
            }
            return null;
        }
        /* ------------------------------------------------------------------------------------- */


        public static Hashtable GetHashtable(this ArrayList arr, int index) {
            if(arr.Count>index && index>=0 && arr[index].GetType() == typeof(Hashtable)) return (Hashtable)arr[index];
            return null;
        }
        public static ArrayList GetArrayList(this ArrayList arr, int index) {
            if(arr.Count>index && index>=0 && arr[index].GetType() == typeof(ArrayList)) return (ArrayList)arr[index];
            return null;
        }
        public static ArrayList GetArrayList(this ArrayList arr, int index, bool wrap) {
            if(arr.Count>index && index>=0) {
                if(arr[index].GetType() == typeof(ArrayList)) return (ArrayList)arr[index];
                else if(wrap) {
                    ArrayList wrapper = new ArrayList();
                    wrapper.Add(arr[index]);
                    return wrapper;
                }
            }
            return null;
        }
        public static string GetString(this ArrayList arr, int index) {
            if(arr.Count>index && index>=0 && arr[index].GetType() == typeof(string)) return (string)arr[index];
            return null;
        }
        public static float GetFloat(this ArrayList arr, int index, float defaultValue = 0f) {
            if(arr.Count>index && index>=0) {
                if(arr[index].GetType() == typeof(float)) return (float)arr[index];
                if(arr[index].GetType() == typeof(int)) return (float)arr[index];
                if(arr[index].GetType() == typeof(string)) return ((string)arr[index]).MakeFloat();
            }
            return defaultValue;
        }

    }
}
