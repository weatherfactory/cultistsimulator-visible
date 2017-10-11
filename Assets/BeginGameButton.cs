using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BeginGameButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textObject;


    public string Text
    {
        get { return textObject.text; }
        set { textObject.text = value; }
    }
}
