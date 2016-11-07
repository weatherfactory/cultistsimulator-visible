using UnityEngine;
using System.Collections;

public class DeactivateOnPlay : MonoBehaviour {
	void Start () {
		gameObject.SetActive(false);
	}
}
