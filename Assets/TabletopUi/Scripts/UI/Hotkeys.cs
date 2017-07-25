using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotkeys : MonoBehaviour
{

    [SerializeField] private DebugTools debugTools;

	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("`"))
            debugTools.gameObject.SetActive(!debugTools.isActiveAndEnabled);
	}
}
