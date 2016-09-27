using UnityEngine;
using System.Collections;
using OrbCreationExtensions;
using UnityEngine.UI;

public class setup : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Hashtable ht = ContentManager.Instance.ImportVerbs();
        ArrayList AllVerbs = ht.GetArrayList("verbs");
        Hashtable v = (Hashtable)AllVerbs[0];
        string description = v.GetString("description");
        ContentManager.Instance.AddToLog(description);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
