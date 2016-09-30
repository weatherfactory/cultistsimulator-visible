using UnityEngine;
using System.Collections;


public class ButtonClicks : MonoBehaviour {

    public void PlusOneElement()
    {
        BoardManager.SetStatusText("clicked");
    }
}
