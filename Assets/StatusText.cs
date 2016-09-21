using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusText : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.GetComponent<Text>().text = ContentManager.Instance.Status;
	}
}
