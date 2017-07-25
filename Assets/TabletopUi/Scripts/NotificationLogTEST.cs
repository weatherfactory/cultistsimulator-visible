using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationLogTEST : MonoBehaviour {
	
	// THIS IS ALL DEBUG STUFF - THIS CLASS CAN GO

	public NotificationLog log;

	void OnEnable() {
	//	StartCoroutine(TestLoop());
	}

	IEnumerator TestLoop() {
		while (true) {
			yield return new WaitForSeconds( Mathf.Lerp(0.5f, 8f, Random.value) );
			log.AddText("Test Random Text. Use this <color=#f6a2e2>Color to highlight</color> special game terms? " + Time.time);
		}
	}

	void OnDisable() {
		StopAllCoroutines();
	}
	// DEBUG STUFF ENDS HERE
}
