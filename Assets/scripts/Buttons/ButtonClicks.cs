using UnityEngine;
using System.Collections;


public class ButtonClicks : BoardMonoBehaviour
{
    
    public void PlusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        BM.ChangeElementQuantityOnBoard(elementId,1);
    }
    public void MinusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        BM.ChangeElementQuantityOnBoard(elementId, -1);
    }

    public void ClearWorkspaceElements()
    {
        BM.ClearWorkspaceElements();
    }
    public void ExecuteCurrentRecipe()
    {
        BM.ExecuteCurrentRecipe();
    }
}
