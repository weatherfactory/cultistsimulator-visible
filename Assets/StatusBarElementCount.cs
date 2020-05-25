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
    public Element Element;


    public void PopulateWithElement(Element element)
    {
        this.gameObject.SetActive(true);
        Element = element;
        if (element.IsAspect)
            ElementImage.sprite = ResourcesManager.GetSpriteForAspectInStatusBar(Element.Icon);
        else
            ElementImage.sprite = ResourcesManager.GetSpriteForAspectInStatusBar(Element.Icon);
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