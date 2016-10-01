using UnityEngine;
using System.Collections;


public class ButtonClicks : BoardMonoBehaviour
{
    
    public void PlusOneElement()
    {
        string elementName = BM.GetDebugElementName();
        BM.SetStatusText(elementName);
    }
}
