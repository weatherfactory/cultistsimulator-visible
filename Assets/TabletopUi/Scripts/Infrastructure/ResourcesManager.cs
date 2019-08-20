using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
using UnityEngine;

public class ResourcesManager: MonoBehaviour
{
    public const string PLACEHOLDER_IMAGE_NAME = "_x";

	public static Sprite GetSpriteForVerbLarge(string verbId)
	{
        return GetSprite("icons100/verbs/", verbId);
    }

	public static Sprite GetSpriteForElement(string imageName)
    {
        return GetSprite("elementArt/", imageName);
    }

    public static Sprite GetSpriteForElement(string imageName, int animFrame) {

        //This doesn't look for the placeholder image: this is intentional (we don't want a flickering pink question mark)
        //but might be a good way to spot missing animations
        return GetSprite("elementArt/anim/", imageName + "_" + animFrame, false);
    }

    public static Sprite GetSpriteForCardBack(string backId) {
        return GetSprite("cardBacks/", backId);
    }

    public static Sprite GetSpriteForAspect(string imageName)
    {
        return GetSprite("icons40/aspects/", imageName);
    }

    public static Sprite GetSpriteForLegacy(string legacyImage)
    {
        return GetSprite("icons100/legacies/", legacyImage, false);
    }

    public static Sprite GetSpriteForDlc(string dlcId, bool active)
    {
        return GetSprite("icons100/dlc/", $"dlc_{dlcId.ToLower()}" + (active ? string.Empty : "-inactive"), false);
    }

    public static Sprite GetSpriteForEnding(string endingImage)
    {
        //just using images from elements for now - LB to sort out rectilinear images if we don't get suitable cards in time

		// Try to load localised version from language subfolder first - if none then fall back to normal one - CP
        Sprite spr = GetSprite(
            "endingArt/" + LanguageTable.targetCulture + "/",
            endingImage,
            false);
		if (spr == null)
		{
			spr = GetSprite("endingArt/", endingImage, false);
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
    
    public static Sprite GetSprite(string folder, string file, bool withPlaceholder = true)
    {
#if MODS
        // Try to find the image in a mod first, in case it overrides an existing one
        var modManager = Registry.Retrieve<ModManager>();
        var modSprite = modManager.GetSprite(folder + file);
        if (modSprite != null)
        {
            return modSprite;
        }
#endif

        // Try to load the image from the packed resources next, and show the placeholder if not found
        var sprite = Resources.Load<Sprite>(folder + file);
        if (sprite != null || !withPlaceholder)
            return sprite;
        return Resources.Load<Sprite>(folder + PLACEHOLDER_IMAGE_NAME);
    }
}

