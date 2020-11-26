/* SimpleJson 1.1                       */
/* April 1, 2015                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* games, components and freelance work */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OrbCreationExtensions;
using UnityEngine.Networking;

public class DemoCtrl : MonoBehaviour {
	private string pathSample1 = "http://orbcreation.com/SimpleJson/Facebook.json";
	private string pathSample2 = "http://orbcreation.com/SimpleJson/YouTube.json";
	private string pathSample3 = "http://orbcreation.com/SimpleJson/Json.org.json";
	private string _downloadedPath = "";
	private string _url;
	private string[] path = new string[0];
	private string _findKey = "";
	private string _findValue = "";
	private Vector2 _scrollPosition = Vector2.zero;
	private Vector2 _scrollPositionLog = Vector2.zero;
	private string _logMsgs = "";
	private string _jsonString;

	private string jsonLogString="";
	private Hashtable jsonNode=null;
	private object selectedNode=null;

	public Texture2D bg;
	public bool caseInsensitive = false;

	private int screenshotCounter = 0;

	/* ------------------------------------------------------------------------------------- */
	/* ----------------------- Using the SimpleJson functions ------------------------------- */

	private Hashtable ImportJsonString(string aString) {
		AddToLog("Importing Json string");
		return SimpleJsonImporter.Import(aString, caseInsensitive);
	}

	private string ExportXml(Hashtable aHash) {
		AddToLog("Exporting Xml string");
		string xmlExportString = aHash.XmlString();
		ExportToFile(xmlExportString, "SimpleJsonExport.xml", true);
		return xmlExportString;
	}

	private string ExportJson(Hashtable aHash) {
		AddToLog("Exporting Json string");
		string jsonExportString = aHash.JsonString();
		ExportToFile(jsonExportString, "SimpleJsonExport.json", false);
		return jsonExportString;
	}

	private void FindNodesWithTagPath(string[] aPath) {
		AddToLog("Looking for first node at path "+ PathToString(aPath));
		selectedNode = jsonNode.GetNodeAtPath(path);

		// the returned object can be of any type
		// only Hashtables and ArrayLists can be converted to a nice readable JSON format
		// so we have to test for the type first
		if(selectedNode==null) {
			AddToLog("Nothing found");
			jsonLogString= "";
		} else if(selectedNode.GetType() == typeof(Hashtable)) {
			AddToLog("Found Hashtable");
			jsonLogString = ((Hashtable)selectedNode).JsonString(false);
		} else if(selectedNode.GetType() == typeof(ArrayList)) {
			AddToLog("Found ArrayList");
			jsonLogString = ((ArrayList)selectedNode).JsonString(false);
		} else {
			AddToLog("Found value");
			jsonLogString = ""+selectedNode;
		}
	}

	private void FindNodesWithProperty(string aKey, string aValue) {
		AddToLog("Looking for first node with \""+aKey + "\" = \"" + aValue + "\"");
		selectedNode = jsonNode.GetNodeWithProperty(_findKey, _findValue);
	    AddToLog(selectedNode == null ? "Nothing found" : "Found Hashtable");
	    // GetNodeWithProperty always returns a Hashtable or null
		// so it is safe to cast it here
		jsonLogString = CreateJsonLogString((Hashtable)selectedNode);
	}

	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ---------------------------------- GUI stuff (not pretty) --------------------------- */

	void Start() {
		_url = pathSample1;
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.P)) StartCoroutine(Screenshot());
	}

	void OnGUI() {
		float margin = 4f;
		float inputLineHeight = 25f;
		float x = margin;
		float y = margin;
		float xColumn2=x;
		float xColumn3=x;
		float xColumn4=x;
		float w = 160f;

		GUI.skin.label.normal.textColor = Color.black;
		GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), bg);

		GUI.Label(new Rect(x,y,w,inputLineHeight), "Download JSON file:");
		x+=w+margin;
		xColumn2 = x;

		w=300f;
		string oldValue = _url;
		_url = GUI.TextField(new Rect(x,y,w,inputLineHeight), _url);
		if(_url!=oldValue) {
			jsonNode=null;
			Reset();
		}
		x+=w+margin;
		xColumn3 = x;

		w=82;
		x=xColumn3;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Example 1")) {
			jsonNode=null;
			Reset();
			_url = pathSample1;
		}
		x+=w+margin;
		w=30;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "2")) {
			jsonNode=null;
			Reset();
			_url = pathSample2;
		}
		x+=w+margin;
		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "3")) {
			jsonNode=null;
			Reset();
			_url = pathSample3;
		}
		y += inputLineHeight;
		xColumn4 = x+w+margin;

		if(_downloadedPath != _url) {
			x=xColumn3;
			w=150;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Download file")) {
				_jsonString = null;
				Reset();
				_downloadedPath = _url;
				StartCoroutine(DownloadJsonFile(_url));
			}
			y += inputLineHeight;
		}


		x=xColumn3;
		w=150;
		if(_downloadedPath == _url && !string.IsNullOrEmpty(_jsonString) && jsonNode == null) {
			y+=10;
			if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Import JSON")) {
				jsonNode = ImportJsonString(_jsonString);

				if(jsonNode==null) {
					AddToLog("Nothing imported");
					jsonLogString = "";
				} else {
					AddToLog("Done importing");
					jsonLogString = CreateJsonLogString(jsonNode);
				}
			}
			y += inputLineHeight;
		}

		if(jsonNode!=null) {
			if(selectedNode == null || selectedNode.GetType() == typeof(Hashtable)) {
				y += 10;
				if(GUI.Button(new Rect(xColumn3,y,w,inputLineHeight), "Export XML")) {
					if(selectedNode != null && selectedNode.GetType() == typeof(Hashtable)) {
						jsonLogString = TruncateStringForEditor(ExportXml((Hashtable)selectedNode));
					} else {
						jsonLogString = TruncateStringForEditor(ExportXml(jsonNode));
					}
				}
				y += inputLineHeight;
				if(GUI.Button(new Rect(xColumn3,y,w,inputLineHeight), "Export JSON")) {
					if(selectedNode != null && selectedNode.GetType() == typeof(Hashtable)) {
						jsonLogString = TruncateStringForEditor(ExportJson((Hashtable)selectedNode));
					} else {
						jsonLogString = TruncateStringForEditor(ExportJson(jsonNode));
					}
				}
				y += inputLineHeight;
			}

			w = 160f;
			x = margin;
			y += inputLineHeight;
			GUI.Label(new Rect(x,y,w,inputLineHeight), "Find by tag :");
			x = xColumn2;
			w=194f;
			int pathIdx = 0;
			if(path==null || path.Length <= 0) path = new string[0];

			if(_url==pathSample1 || _url==pathSample2 || _url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin,y,75,inputLineHeight), "Example 1")) {
					if(_url==pathSample1) {
						path = new string[2] {"data", "message"};
					} else if(_url==pathSample2) {
						path = new string[3] {"data", "items", "tags"};
					} else if(_url==pathSample3) {
						path = new string[2] {"web-app", "taglib"};
					}
				}
			}
			if(_url==pathSample1 || _url==pathSample2 || _url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin+75+margin,y,22,inputLineHeight), "2")) {
					if(_url==pathSample1) {
						path = new string[3] {"data", "from", "name"};
					} else if(_url==pathSample2) {
						path = new string[3] {"data", "items", "content"};
					} else if(_url==pathSample3) {
						path = new string[3] {"web-app", "servlet", "servlet-name"};
					}
				}
			}

			for(pathIdx=0; pathIdx<path.Length && path[pathIdx].Length>0; pathIdx++) {
				path[pathIdx] = GUI.TextField(new Rect(x+(pathIdx * 10),y,w,inputLineHeight), path[pathIdx]);
				y += inputLineHeight;
			}
			string newPathEntry="";
			if(path.Length<10) {
				newPathEntry = GUI.TextField(new Rect(x+(pathIdx * 10),y,w,inputLineHeight), "");
				if(newPathEntry.Length>0) pathIdx++;
			}
			if(pathIdx!=path.Length) {
				string[] newPath = new string[pathIdx];
        		Array.Copy(path, newPath, Mathf.Min(path.Length, pathIdx));
        		if(newPathEntry.Length>0) newPath[pathIdx-1] = newPathEntry;
        		path = newPath;
			}
			x=xColumn3;
			w=150;
			if(path.Length>0) {
				if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Find 1st occurence")) {
					FindNodesWithTagPath(path);
					_findKey = "";
					_findValue = "";
				}
			}

			w = 160f;
			x = margin;
			y += inputLineHeight;
			y += inputLineHeight;
			GUI.Label(new Rect(x,y,w,inputLineHeight), "Find by property:");
			x = xColumn2;
			w=90f;
			_findKey = GUI.TextField(new Rect(x,y,w,inputLineHeight), _findKey);
			x+=w;
			GUI.Label(new Rect(x,y,15,inputLineHeight), " =");
			x+=15;
			_findValue = GUI.TextField(new Rect(x,y,w,inputLineHeight), _findValue);

			if(_url==pathSample1 || _url==pathSample2 || _url==pathSample3) {
				if(GUI.Button(new Rect(x+w+margin,y,75,inputLineHeight), "Example 1")) {
					if(_url==pathSample1) {
						_findKey = "id";
						_findValue = "X998_Y998";
					} else if(_url==pathSample2) {
						_findKey = "id";
						_findValue = "hYB0mn5zh2c";
					} else if(_url==pathSample3) {
						_findKey = "servlet-name";
						_findValue = "cofaxTools";
					}
				}
				if(GUI.Button(new Rect(x+w+margin+75+margin,y,22,inputLineHeight), "2")) {
					if(_url==pathSample1) {
						_findKey = "name";
						_findValue = "Tom Brady";
					} else if(_url==pathSample2) {
						_findKey = "commentVote";
						_findValue = "allowed";
					} else if(_url==pathSample3) {
						_findKey = "betaServer";
						_findValue = "true";
					}
				}
			}
			w=150f;
			x=xColumn3;
			if(_findKey.Length>0 && _findValue.Length>0) {
				if(GUI.Button(new Rect(x,y,w,inputLineHeight), "Find 1st occurence")) {
					path=null;
					FindNodesWithProperty(_findKey, _findValue);
				}
			}

			y += inputLineHeight;
			if(GUI.Button(new Rect(xColumn3,y,w,inputLineHeight), "Reset")) Reset();
			y += inputLineHeight;
		}

		y += margin;
		x = margin;
		w = xColumn4-margin-x;
		float h = GUI.skin.label.CalcHeight(new GUIContent(_logMsgs), w-25);
        _scrollPositionLog = GUI.BeginScrollView(new Rect(x, y, w, Screen.height-y-margin), _scrollPositionLog, new Rect(0, 0, w-25, h));
        GUI.Label(new Rect(0, 0, w-25, h), _logMsgs);
        GUI.EndScrollView();

		x = xColumn4;
		y = margin;
		w = Mathf.Clamp(Screen.width-xColumn4-margin, 100, Screen.width);
		h = GUI.skin.label.CalcHeight(new GUIContent(jsonLogString), w-25);
        _scrollPosition = GUI.BeginScrollView(new Rect(x, y, w, Screen.height-y-margin), _scrollPosition, new Rect(0, 0, w-25, h));
        GUI.Label(new Rect(0, 0, w-25, h), jsonLogString);
        GUI.EndScrollView();

	}

	private void Reset() {
		Debug.Log("Resetting");
		jsonLogString = "";
		if(jsonNode!=null) jsonLogString = CreateJsonLogString(jsonNode);
		path = new string[0];
		_findKey = "";
		_findValue = "";
		selectedNode = null;
	}

	// To make the screenshots used for the Asset Store submission
	private IEnumerator Screenshot() {
		yield return new WaitForEndOfFrame(); // wait for end of frame to include GUI

		Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenshot.Apply(false);

		if(Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.WindowsPlayer && Application.platform!=RuntimePlatform.LinuxPlayer || Application.isEditor) {
			byte[] bytes = screenshot.EncodeToPNG();
			FileStream fs = new FileStream("Screenshot"+screenshotCounter+".png", FileMode.OpenOrCreate);
			BinaryWriter w = new BinaryWriter(fs);
			w.Write(bytes);
			w.Close();
			fs.Close();
		}
		screenshotCounter++;

	}
	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Downloading files  ---------------------------------- */

	private IEnumerator DownloadJsonFile(string url) {
		_jsonString = null;
		yield return StartCoroutine(DownloadFile(url, fileContents => _jsonString = fileContents));
// 		Debug.Log(jsonString);
		jsonLogString = TruncateStringForEditor(_jsonString);
	}

	private IEnumerator DownloadFile(string url, System.Action<string> result) {
		AddToLog("Downloading "+url);
	    UnityWebRequest www = new UnityWebRequest(url);
        yield return www;
        if(www.error!=null) {
        	AddToLog(www.error);
        } else {
        	AddToLog("Downloaded "+www.downloadedBytes+" bytes");
        }
       	result(www.downloadHandler.text);
	}

	private void ExportToFile(string exportString, string path, bool addXmlProlog) {
		if(exportString == null || path == null) return;
		if(Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.WindowsPlayer && Application.platform!=RuntimePlatform.LinuxPlayer || Application.isEditor) {
			if(addXmlProlog) {
				// Put a proper prolog in the file
				exportString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + exportString;
			}

			// We use UTF-8 encoding
	        byte[] bytes = new UTF8Encoding(true).GetBytes(exportString);
	        if(File.Exists(path)) File.Delete(path); // delete if it exists
 			FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
			BinaryWriter w = new BinaryWriter(fs);
			w.Write(bytes);
			w.Close();
			fs.Close();
        }
    }


	/* ------------------------------------------------------------------------------------- */



	/* ------------------------------------------------------------------------------------- */
	/* ------------------------------- Logging functions  ---------------------------------- */

	private void AddToLog(string msg) {
		Debug.Log(msg+"\n"+DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));

		// for some silly reason the Editor will generate errors if the string is too long
		int lenNeeded = msg.Length + 1;
		if(_logMsgs.Length + lenNeeded>4096) _logMsgs = _logMsgs.Substring(0,4096-lenNeeded);

		_logMsgs = _logMsgs + "\n" + msg;
	}
    private string PathToString(string[] aPath) {
   		string str = "{";
   		for(int i=0;i<aPath.Length;i++) {
   			str = str + aPath[i];
   			if(i<aPath.Length-1) str = str + ", ";
   		}
   		str = str + "}";
   		return str;
    }

 	private string CreateJsonLogString(Hashtable aNode) {
 		if(aNode == null) return "";
 		string aStr = aNode.JsonString().Replace("\t", "  ");
		return TruncateStringForEditor(aStr);
	}

	private string CreateJsonLogString(ArrayList aNodeArray) {
 		if(aNodeArray == null) return "";
 		string aStr = aNodeArray.JsonString().Replace("\t", "  ");
		return TruncateStringForEditor(aStr);
	}

    private string TruncateStringForEditor(string str) {
    	// for some silly reason the Editor will generate errors if the string is too long
		if(str.Length>4096) str = str.Substring(0,4000)+"\n .... display truncated ....\n";
		return str;
    }
	/* ------------------------------------------------------------------------------------- */

}
