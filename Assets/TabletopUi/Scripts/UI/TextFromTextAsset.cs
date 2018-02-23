using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFromTextAsset : MonoBehaviour {

    public string textAssetName;
    public TextMeshProUGUI textMesh;

	void Start () {
        var textAsset = Resources.Load<TextAsset>(textAssetName);

        if (textAsset != null && textMesh != null)
            textMesh.text = textAsset.text;
	}
	
}
