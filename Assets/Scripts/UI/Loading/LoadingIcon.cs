using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour {

	public float rotationSpeed = 1f;
	public bool doRotation = true;

	void Update () {
		if (doRotation)
			transform.rotation = Quaternion.Euler(0f, 0f, Time.time * rotationSpeed);
	}
}
