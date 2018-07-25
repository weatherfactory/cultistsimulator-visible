using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuSubtitle : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI SubtitleText;

    [SerializeField] public TextMeshProUGUI SubtitleTextShadow;

    public void SetText(string text)
    {
        SubtitleText.text = text;
        SubtitleTextShadow.text = text;
    }
}
