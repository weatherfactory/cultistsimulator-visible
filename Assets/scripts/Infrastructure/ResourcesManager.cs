using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Martin pointed out this should be static; he's probably right
/// </summary>
    public class ResourcesManager:MonoBehaviour
    {
    public Sprite GetSpriteForVerb(string verbId)
    {
        return Resources.Load<Sprite>("icons40/verbs/" + verbId);
    }

    public Sprite GetSpriteForElement(string elementId)
    {
        return Resources.Load<Sprite>("FlatIcons/png/32px/" + elementId);
    }
}

