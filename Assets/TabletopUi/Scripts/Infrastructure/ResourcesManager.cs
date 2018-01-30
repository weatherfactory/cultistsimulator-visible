using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResourcesManager: MonoBehaviour
{
    private const string PLACEHOLDER_IMAGE_NAME = "_x";

	public static Sprite GetSpriteForVerbLarge(string verbId)
	{

        var sprite = Resources.Load<Sprite>("icons100/verbs/" + verbId);
        if (sprite == null)
            return Resources.Load<Sprite>("icons100/verbs/" + PLACEHOLDER_IMAGE_NAME);
        else
            return sprite;
    }

	public static Sprite GetSpriteForElement(string elementId)
    {
        var sprite = Resources.Load<Sprite>("elementArt/" + elementId);
        if (sprite == null)
            sprite = Resources.Load<Sprite>("elementArt/" + PLACEHOLDER_IMAGE_NAME);
        return sprite;
    }

    public static Sprite GetSpriteForElement(string elementId, int animFrame) {

        //This doesn't look for the placeholder image: this is intentional (we don't want a flickering pink question mark)
        //but might be a good way to spot missing animations
        return Resources.Load<Sprite>("elementArt/anim/" + elementId + "_" + animFrame);
    }

    public static Sprite GetSpriteForCardBack(string backId) {
        return Resources.Load<Sprite>("cardBacks/" + backId);
    }

    public static Sprite GetSpriteForAspect(string aspectId)
    {
        var sprite= Resources.Load<Sprite>("icons40/aspects/" + aspectId);
        if(sprite==null)
            sprite= Resources.Load<Sprite>("icons40/aspects/" + PLACEHOLDER_IMAGE_NAME);
        return sprite;
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

        public static IEnumerable<AudioClip> GetImpendingDoomMusic()
        {
            return Resources.LoadAll<AudioClip>("music/impendingdoom/");
        }
}

