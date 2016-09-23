using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ContentClasses;
using OrbCreationExtensions;

public class StatusText : MonoBehaviour {
    
	// Use this for initialization
	void Start ()
	{
	    Hashtable ht = ContentManager.Instance.ImportVerbs();
	    ArrayList AllVerbs = ht.GetArrayList("verbs");
	    Hashtable v = (Hashtable)AllVerbs[0];
	    string description = v.GetString("description");
	    gameObject.GetComponent<Text>().text = description;



	}
	
	// Update is called once per frame
	void Update ()
	{
	

	}
}
