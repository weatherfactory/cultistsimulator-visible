using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

    public class ResourcesManager: MonoBehaviour
    {
    public static Sprite GetSpriteForVerb(string verbId)
    {
        return Resources.Load<Sprite>("icons40/verbs/" + verbId);
	}
	public static Sprite GetSpriteForVerbLarge(string verbId)
	{
		return Resources.Load<Sprite>("icons100/verbs/100" + verbId);
	}

	public static Sprite GetSpriteForElement(string elementId)
    {
        return Resources.Load<Sprite>("FlatIcons/png/32px/" + elementId);
    }

    public static Sprite GetSpriteForAspect(string aspectId)
    {
        return Resources.Load<Sprite>("icons40/aspects/" + aspectId);
    }
}

