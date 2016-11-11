using UnityEngine;
using System.Collections;

/// <summary>
/// displays the verb needed for a recipe
/// </summary>
public class VerbNeeded : BoardMonoBehaviour {

    public void ClearVerb()
    {
        if(transform.childCount>0)
            BM.ExileToLimboThenDestroy(transform.GetChild(0).gameObject);
    }

    public void SetVerb(string verbId)
    {
        ClearVerb();
        Verb verb = Heart.Compendium.GetVerbById(verbId);
        BM.AddVerbTokenToParent(verb, gameObject.transform);
    }
}
