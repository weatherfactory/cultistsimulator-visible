using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void SetFirstElementVisibility(bool visibility)
    {
        GameObject.Find("pnlFirstElementSlot").GetComponent<CanvasGroup>().alpha = Convert.ToInt32(visibility);
    }
    public static void SetStatusText(string statusText)
    {
        GameObject txtStatus = GameObject.Find("txtStatus");
        txtStatus.GetComponent<Text>().text = statusText;
    }
}
