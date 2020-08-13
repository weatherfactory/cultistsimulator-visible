using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using UnityEngine;

public class ResourcesManager: MonoBehaviour
{

    public const string PLACEHOLDER_IMAGE_NAME = "_x";

    private static readonly Dictionary<string,List<Sprite>> _cachedVerbFrames=new Dictionary<string, List<Sprite>>();
    private static readonly Dictionary<string, List<Sprite>> _cachedElementFrames = new Dictionary<string, List<Sprite>>();


    public static Sprite GetSpriteForVerbLarge(string verbId)
	{
        return GetSprite("verbs", verbId);
    }


    public static List<Sprite> GetAnimFramesForVerb(string verbId)
    {
        if(_cachedVerbFrames.ContainsKey(verbId))
            return _cachedVerbFrames[verbId];
        

        List<Sprite> frames=new List<Sprite>();

        int i = 0;

        while (true)
        {
            var s= GetSprite("verbs\\anim", verbId + "_" + i, false);
            if (s != null)
            {
                frames.Add(s);
                i++;
            }
            else
            {
                break;
            }
        }

        _cachedVerbFrames[verbId] = frames;

        return frames;
    }


    public static Sprite GetSpriteForElement(string imageName)
    {
        return GetSprite("elements", imageName);
    }

    public static Sprite GetSpriteForAspectInStatusBar(string imageName)
    {
        return GetSprite("statusbaricons", imageName);
    }

    public static Sprite GetSpriteForElement(string imageName, int animFrame) {

        //This doesn't look for the placeholder image: this is intentional (we don't want a flickering pink question mark)
        //but might be a good way to spot missing animations
        return GetSprite("elements\\anim", imageName + "_" + animFrame, false);
    }

    public static List<Sprite> GetAnimFramesForElement(string imageName)
    {
        if (_cachedElementFrames.ContainsKey(imageName))
            return _cachedElementFrames[imageName];


        List<Sprite> frames = new List<Sprite>();

        int i = 0;

        while (true)
        {
            var s = GetSprite("elements/anim", imageName + "_" + i, false);
            if (s != null)
            {
                frames.Add(s);
                i++;
            }
            else
            {
                break;
            }
        }

        _cachedElementFrames[imageName] = frames;

        return frames;
    }


    public static Sprite GetSpriteForCardBack(string backId) {
        return GetSprite("cardbacks/", backId);
    }

    public static Sprite GetSpriteForAspect(string imageName)
    {
        return GetSprite("aspects", imageName);
    }

    public static Sprite GetSpriteForLegacy(string legacyImage)
    {
        return GetSprite("legacies", legacyImage, false);
    }


    public static Sprite GetSpriteForUI(string uiImage)
    {
        return GetSprite("ui", uiImage, false);
    }

    public static Sprite GetSpriteForEnding(string endingImage)
    {
        //just using images from elements for now - LB to sort out rectilinear images if we don't get suitable cards in time

		// Try to load localised version from language subfolder first - if none then fall back to normal one - CP
     return GetSpriteLocalised(
            "endings",
            endingImage,
            LanguageManager.targetCulture,
            false);

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
        var spritePath = Path.Combine("images", folder, file);

        // Try to find the image in a mod first, in case it overrides an existing one
        var modManager = Registry.Retrieve<ModManager>();
        var modSprite = modManager.GetSprite(spritePath);
        if (modSprite != null)
        {
            return modSprite;
        }

        // Try to load the image from the packed resources next, and show the placeholder if not found
        var sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null || !withPlaceholder)
            return sprite;
        return Resources.Load<Sprite>(spritePath.Replace(file, PLACEHOLDER_IMAGE_NAME));
    }


    public static Sprite GetSpriteLocalised(string folder, string file, string cultureId, bool withPlaceholder = true)
    {
        Sprite spriteToReturn=null;

        if (LanguageManager.targetCulture != NoonConstants.DEFAULT_CULTURE)
        {

            var spritePath = Path.Combine("images", folder,
                NoonConstants.LOC_FOLDER_TEMPLATE.Replace(NoonConstants.LOC_TOKEN, cultureId), file);

            // Try to find the image in a mod first, in case it overrides an existing one
            var modManager = Registry.Retrieve<ModManager>();
            spriteToReturn = modManager.GetSprite(spritePath);
            if (spriteToReturn != null)
            {
                return spriteToReturn;
            }

            // Try to load the image from the packed resources next. Never show the placeholder: we'll fall back to core-loc image if appropriate
            spriteToReturn = Resources.Load<Sprite>(spritePath);
        }

        if (spriteToReturn == null)
            return GetSprite(folder, file, withPlaceholder);
        else
            return spriteToReturn;

    }
}

