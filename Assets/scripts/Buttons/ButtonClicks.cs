using UnityEngine;
using System.Collections;


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
        PlayerPrefs.SetString("TestString","Foo");
        BM.Log("Saved");
    }
    public void Load()
    {
        BM.Log(PlayerPrefs.GetString("TestString"));
    }
}
