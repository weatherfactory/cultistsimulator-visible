using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialInstancer : MonoBehaviour
{
    void Start()
    {
		#if UNITY_EDITOR
		// Not needed in release but this saves constantly reverting tweaked materials before committing
		
		Renderer R = gameObject.GetComponent<Renderer>() as Renderer;
		if (R!=null)
		{
			Debug.Log("Instanced " + R.sharedMaterial.name);
			R.sharedMaterial = new Material(R.sharedMaterial);
		}

		Image im = gameObject.GetComponent<Image>() as Image;
		if (im!=null)
		{
			Debug.Log("Instanced " + im.material.name);
			im.material = new Material(im.material);
		}
		#endif
    }
}
