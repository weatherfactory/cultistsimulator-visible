using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using UnityEngine;

public class ResourcesManager: MonoBehaviour
{
    public const string PLACEHOLDER_IMAGE_NAME = "_x";

	public static Sprite GetSpriteForVerbLarge(string verbId)
	{

        var sprite = Resources.Load<Sprite>("icons100/verbs/" + verbId);
        if (sprite == null)
            return Resources.Load<Sprite>("icons100/verbs/" + PLACEHOLDER_IMAGE_NAME);
        else
            return sprite;
    }

	public static Sprite GetSpriteForElement(string imageName)
    {
        var sprite = Resources.Load<Sprite>("elementArt/" + imageName);
        if (sprite == null)
            sprite = Resources.Load<Sprite>("elementArt/" + PLACEHOLDER_IMAGE_NAME);
        return sprite;
    }

    public static Sprite GetSpriteForElement(string imageName, int animFrame) {

        //This doesn't look for the placeholder image: this is intentional (we don't want a flickering pink question mark)
        //but might be a good way to spot missing animations
        return Resources.Load<Sprite>("elementArt/anim/" + imageName + "_" + animFrame);
    }

    public static Sprite GetSpriteForCardBack(string backId) {
        var sprite = Resources.Load<Sprite>("cardBacks/" + backId);

        if (sprite == null)
            sprite = Resources.Load<Sprite>("cardBacks/" + PLACEHOLDER_IMAGE_NAME);

        return sprite;
    }

    public static Sprite GetSpriteForAspect(string imageName)
    {
        var sprite = Resources.Load<Sprite>("icons40/aspects/" + imageName);

        if (sprite == null)
            sprite = Resources.Load<Sprite>("icons40/aspects/" + PLACEHOLDER_IMAGE_NAME);

        return sprite;
    }
        public static Sprite GetSpriteForLegacy(string legacyImage)
        {
            return Resources.Load<Sprite>("icons100/legacies/" + legacyImage);
        }


    public static Sprite GetSpriteForEnding(string endingImage)
    {
        //just using images from elements for now - LB to sort out rectilinear images if we don't get suitable cards in time

		// Try to load localised version from language subfolder first - if none then fall back to normal one - CP
		Sprite spr = Resources.Load<Sprite>("endingArt/" + LanguageTable.targetCulture + "/" + endingImage);
		if (spr == null)
		{
			spr = Resources.Load<Sprite>("endingArt/" + endingImage);
		}
		
		return spr;
    }

    public static IEnumerable<AudioClip> GetBackgroundMusic()
    {
        return Resources.LoadAll<AudioClip>("music/background");
    }

    public static IEnumerable<AudioClip> GetImpendingDoomMusic()
    {
        return Resources.LoadAll<AudioClip>("music/impendingdoom");
    }

    public static IEnumerable<AudioClip> GetMansusMusic() {
        return Resources.LoadAll<AudioClip>("music/mansus");
    }

    public static IEnumerable<AudioClip> GetEndingMusic(EndingFlavour endingFlavour)
    {
        if (endingFlavour == EndingFlavour.Grand)
            return Resources.LoadAll<AudioClip>("music/endings/grand");

        if (endingFlavour == EndingFlavour.Melancholy)
            return Resources.LoadAll<AudioClip>("music/endings/melancholy");

        if (endingFlavour == EndingFlavour.Pale)
            return Resources.LoadAll<AudioClip>("music/endings/melancholy");


        if (endingFlavour == EndingFlavour.Vile)
            return Resources.LoadAll<AudioClip>("music/endings/melancholy");
        else
            return null;
    }
}

