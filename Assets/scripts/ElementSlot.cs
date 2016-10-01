using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ElementSlot : MonoBehaviour {

    public void SetElementId(string id)
    {
        Text NameText = GetComponentsInChildren<Text>()[0];
        NameText.text = id;
    }
}
