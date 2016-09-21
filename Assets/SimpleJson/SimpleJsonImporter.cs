/* SimpleJson 1.1                       */
/* April 1, 2015                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* games, components and freelance work */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OrbCreationExtensions;

public class SimpleJsonImporter {

	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Public Import interface ----------------------------- */
	// simple case sensitive import of the full xml string
	public static Hashtable Import(string json) {
		return Import(json, false);
	}


	// importing case insensitive will turn all tags into lowercase
	public static Hashtable Import(string json, bool caseInsensitive) {
		int end = json.Length;
		int begin = 0;
		MoveToNextNode(json, ref begin, end);
		if(begin<end) {
			int nodeEnd = FindMatchingEnd(json, begin, end);
			if(json[begin] == '{') return ReadHashtable(json, ref begin, nodeEnd, caseInsensitive);
			else if(json[begin] == '[') {
				ArrayList list = ReadArrayList(json, ref begin, nodeEnd, caseInsensitive);
				if(list != null && list.Count > 0) {
					Hashtable wrapper = new Hashtable();
					wrapper.Add("SimpleJSON", list);
					return wrapper;
				}
			}
		}
		return null;
	}
	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- private Import functions ---------------------------- */
	private static Hashtable ReadHashtable(string json, ref int begin, int end, bool caseInsensitive) {
		Hashtable retval = new Hashtable();
		int idx;
		int isReading = 1;   // 1 key, 2 value
		bool withinQuotes = false;
		string ignoreCharsKey = "\r\n\t ?\"'\\,:{}[]";
		string key = "";
		string value = "";
		for(idx=begin+1; idx<end; idx++) {
			bool skipThisChar = false;
			char c=json[idx];
			if(idx==0 || json[idx-1] != '\\') {
				if(c == '\"') withinQuotes = (!withinQuotes);
				if(!withinQuotes) {
					if(isReading != 1 && c == ',') {  // end of entry
						// add the previous key and value if not yet present
						value = TrimPropertyValue(value);
						if(key.Length > 0 && (!retval.ContainsKey(key)) && value.Length>0) {
							retval[key] = value.JsonDecode();
						}
						isReading = 1;  // start reading next key
						key = "";
						value = "";
						skipThisChar = true;
					}
					if(isReading == 1 && c == ':') {  // end of key
						isReading = 2;  // start reading value
						value = "";
						skipThisChar = true;
					}
					if(isReading == 2 && c == '{') {  // value is a hashtable
						int nodeEnd = FindMatchingEnd(json, idx, end);
						retval[key] = ReadHashtable(json, ref idx, nodeEnd, caseInsensitive);
						value = "";
						isReading = 0;
						skipThisChar = true;
					}
					if(isReading == 2 && c == '[') {  // value is a arraylist
						int nodeEnd = FindMatchingEnd(json, idx, end);
						retval[key] = ReadArrayList(json, ref idx, nodeEnd, caseInsensitive);
						value = "";
						isReading = 0;
						skipThisChar = true;
					}
				}
			}

			if(!skipThisChar) {
				// read the char into the current key or the value
				if(isReading == 1 && ignoreCharsKey.IndexOf(c)<0) key = key + c;
				if(isReading == 2) value = value + c;
			}
		}
		// add the last key and value if not yet present
		if(key.Length > 0 && (!retval.ContainsKey(key))) {
			value = TrimPropertyValue(value);
			if(value.Length>0) retval[key] = value.JsonDecode();
		}
		begin = idx;
		return retval;
	}

	private static ArrayList ReadArrayList(string json, ref int begin, int end, bool caseInsensitive) {
		ArrayList retval = new ArrayList();
		int idx;
		bool withinQuotes = false;
		string value = "";
		for(idx=begin+1; idx<end; idx++) {
			bool skipThisChar = false;
			char c=json[idx];
			if(idx==0 || json[idx-1] != '\\') {
				if(c == '\"') withinQuotes = (!withinQuotes);
				if(!withinQuotes) {
					if(c == '{') {  // value is a hashtable
						int nodeEnd = FindMatchingEnd(json, idx, end);
						retval.Add(ReadHashtable(json, ref idx, nodeEnd, caseInsensitive));
						value = "";
						skipThisChar = true;
					} else if(c == '[') {  // value is a arraylist
						int nodeEnd = FindMatchingEnd(json, idx, end);
						retval.Add(ReadArrayList(json, ref idx, nodeEnd, caseInsensitive));
						value = "";
						skipThisChar = true;
					} else if(c == ',') {  // end of entry
						value = TrimPropertyValue(value);
						if(value.Length > 0) retval.Add(value.JsonDecode());  // add the previous value
						value = "";
						skipThisChar = true;
					}
				}
			}
			if(!skipThisChar) {
				// read the char into the current value
				value = value + c;
			}
		}
		// add the last value
		value = TrimPropertyValue(value);
		if(value.Length > 0) retval.Add(value.JsonDecode());
		begin = idx;
		return retval;
	}

	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- private helper functions ---------------------------- */

	private static void MoveToNextNode(string json, ref int begin, int end) {
		int idx;
		bool withinQuotes = false;
		for(idx=begin; idx<end; idx++) {
			char c=json[idx];
			if(c == '\"' && idx > 0 && json[idx-1] != '\"') {
				withinQuotes = (!withinQuotes);
			}
			if(!withinQuotes) {
				if(c == '{' || c=='[') {
					begin = idx;
					return;
				}
			}
		}
		begin=end;   // not found
		return;
	}

	private static int FindMatchingEnd(string json, int begin, int end) {
		int nestingLevel = 0;
		bool withinQuotes = false;
		for(int idx=begin; idx<end; idx++) {
			char c=json[idx];
			if(idx==0 || json[idx-1] != '\\') {
				if(c == '\"') withinQuotes = (!withinQuotes);
				else if(c== '{' || c == '[') nestingLevel++;
				else if(c== '}' || c == ']') {
					nestingLevel--;
					if(nestingLevel==0) return idx;   // if we are at the correct nesting level, we found the end
				}
			}
		}
		return end;
	}

	// remove all leading and trailing <space>, <tab>, <newline>, <return> and <doublequote>
	private static string TrimPropertyValue(string str) {
		str = str.Trim();
		if(str==null || str.Length==0) return "";
		while(str.Length>1 && str[0]=='\r' || str[0]=='\n' || str[0]=='\t' || str[0]==' ') str = str.Substring(1,str.Length-1);
		while(str.Length>0 && str[str.Length-1]=='\r' || str[str.Length-1]=='\n' || str[str.Length-1]=='\t' || str[str.Length-1]==' ') str = str.Substring(0,str.Length-1);
		if(str==null) return "";
		if(str.Length>=2 && str[0] == '"' && str[str.Length-1] == '"') return str.Substring(1,str.Length-2);
		return str;
	}
	/* ------------------------------------------------------------------------------------- */
}
