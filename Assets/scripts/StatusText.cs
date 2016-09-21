using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusText : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    Hashtable actionsTable = ContentManager.Instance.ImportActionsFromJSON();
        foreach(DictionaryEntry pair in actionsTable)
        {
            gameObject.GetComponent<Text>().text = pair.Value.ToString();
        }

    }
}
