using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarElementCount : MonoBehaviour
{

    [SerializeField] public Image ElementImage;
    [SerializeField] public TextMeshProUGUI ElementCount;
    [SerializeField] public TextMeshProUGUI FatiguedElementCount;


    public void PopulateImageForElement(string elementId)
    {
        ElementImage.sprite= ResourcesManager.GetSpriteForElement(elementId);
    }

    public void SetCount(int count)
    {
        var color = (count == 0 ? Color.red : Color.white);
        ElementCount.text = count.ToString();
        ElementCount.color= color;
    }

    public void SetFatiguedCount(int fatiguedCount)
    {
        if(fatiguedCount>0)
            FatiguedElementCount.text = "(" + fatiguedCount.ToString() + ")";
        else
            FatiguedElementCount.text=String.Empty;
    }
}