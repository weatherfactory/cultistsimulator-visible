using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager_Script : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log ("Starting");
	}
	
	// Update is called once per frame
	//void Update () {
	
	//}
	public void NewGame()
	{
		Debug.Log ("New");
		SceneManager.LoadScene ("board");
	}
	public void ExitGame()
	{
		Debug.Log ("Exiting");
		Application.Quit ();
	}
}
