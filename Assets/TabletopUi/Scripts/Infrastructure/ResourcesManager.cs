using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResourcesManager: MonoBehaviour
    {

	public static Sprite GetSpriteForVerbLarge(string verbId)
	{

        var sprite = Resources.Load<Sprite>("icons100/verbs/" + verbId);
        if (sprite == null)
            return Resources.Load<Sprite>("icons100/verbs/x");
        else
            return sprite;
    }

	public static Sprite GetSpriteForElement(string elementId)
    {
        return Resources.Load<Sprite>("elementArt/" + elementId);
    }

    public static Sprite GetSpriteForElement(string elementId, int animFrame) {
        return Resources.Load<Sprite>("elementArt/anim/" + elementId + "_" + animFrame);
    }

    public static Sprite GetSpriteForCardBack(string backId) {
        return Resources.Load<Sprite>("cardBacks/" + backId);
    }

    public static Sprite GetSpriteForAspect(string aspectId)
    {
        return Resources.Load<Sprite>("icons40/aspects/" + aspectId);
    }
        public static Sprite GetSpriteForLegacy(string legacyImage)
        {
            return Resources.Load<Sprite>("icons100/legacies/" + legacyImage);
        }


    public static Sprite GetSpriteForEnding(string endingImage)
    {
        //just using images from elements for now - LB to sort out rectilinear images if we don't get suitable cards in time
        return Resources.Load<Sprite>("elementArt/" + endingImage);

    }

    public static IEnumerable<AudioClip> GetBackgroundMusic()
    {
        return Resources.LoadAll<AudioClip>("music/");
    }
}

