using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFromTextAsset : MonoBehaviour {

    public string textAssetName;
    public TextMeshProUGUI textMesh;

	void Start () {
        var textAsset = Resources.Load<TextAsset>(textAssetName);

        if (textAsset == null)
            Debug.LogWarning("[TextFromTextAsset] Could not load Resources/" + textAssetName);
        else if (textMesh == null)
            Debug.LogWarning("[TextFromTextAsset] Loaded text asset but no text mesh to use specified.");
        else 
            textMesh.text = textAsset.text;
	}
	
}
