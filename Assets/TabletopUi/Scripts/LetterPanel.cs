using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterPanel : MonoBehaviour {

    public void Show()
    {
     gameObject.SetActive(true);
    }

    public void Hide()
    {
     gameObject.SetActive(false);

    }
}
