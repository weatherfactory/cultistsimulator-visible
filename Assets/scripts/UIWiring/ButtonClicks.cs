using System;
using UnityEngine;
using System.Collections;
using System.IO;
using OrbCreationExtensions;


public class ButtonClicks : BoardMonoBehaviour
{

    public void PlusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        if (ContentRepository.Instance.GetElementById(elementId) == null)
            BM.Log("Can't find element with id " + elementId, Style.Assertive);
        else
        BM.ModifyElementQuantity(elementId, 1);
    }

    public void MinusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        if(ContentRepository.Instance.GetElementById(elementId)==null)
            BM.Log("Can't find element with id " + elementId,Style.Assertive);
        else
            BM.ModifyElementQuantity(elementId, -1);
    }

    public void ClearWorkspaceElements()
    {
        BM.ClearWorkspaceElements();
    }

    public void QueueCurrentRecipe()
    {
        BM.QueueCurrentRecipe();
    }

    public void Save()
    {
        BM.SaveCurrentBoard();
    }

    public void Load()
    {
        BM.LoadCurrentBoard();
    }

    public void FastForward30()
    {
        BM.FastForward(30);
    }
}
