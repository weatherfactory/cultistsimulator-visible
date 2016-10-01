using UnityEngine;
using System.Collections;


public class ButtonClicks : BoardMonoBehaviour
{
    
    public void PlusOneElement()
    {
        string elementId = BM.GetDebugElementName();
        BM.SetStatusText(elementId);
        BM.AddElementToBoard(elementId);
    }
}
