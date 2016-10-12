using UnityEngine;
using System.Collections;

public class Workspace : BoardMonoBehaviour
{

    [SerializeField] private SlotReceiveVerb VerbSlot;

    public string GetCurrentVerbId()
    {
        return VerbSlot.GetCurrentVerbId();
    }
}
