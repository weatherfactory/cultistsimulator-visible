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
        BM.ModifyElementQuantityOnBoard(elementId,1);
    }
    public void MinusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        BM.ModifyElementQuantityOnBoard(elementId, -1);
    }

    public void ClearWorkspaceElements()
    {
        BM.ClearWorkspaceElements();
    }
    public void ExecuteCurrentRecipe()
    {
        BM.QueueCurrentRecipe();
    }

    public void Save()
    {
        string exportJson;
        
        Hashtable htElementsPossessed=new Hashtable();
        foreach (DraggableElementToken e in BM.GetAllStoredElementTokens())
        {
            htElementsPossessed.Add(e.Element.Id,e.Quantity);
        }
        exportJson= htElementsPossessed.JsonString();
        System.IO.File.WriteAllText(Noon.NoonUtility.GetGameSavePath(), exportJson);
        BM.Log("Saved");
    }

    public void Load()
    {
        string importJson=File.ReadAllText(Noon.NoonUtility.GetGameSavePath());
        Hashtable htElementsPossessed = SimpleJsonImporter.Import(importJson);


        //check if it's all valid first
        BM.ClearBoard();

        foreach (string k in htElementsPossessed.Keys)
        {
            BM.ModifyElementQuantityOnBoard(k,Convert.ToInt32(htElementsPossessed[k]));
        }

        BM.Log("loaded");
    }
}
