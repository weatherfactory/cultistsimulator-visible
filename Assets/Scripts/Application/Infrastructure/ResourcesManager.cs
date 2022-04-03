﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants.Modding;
using SecretHistories.Services;

using UnityEngine;

public class ResourcesManager: MonoBehaviour
{

    public const string PLACEHOLDER_IMAGE_NAME = "_x";
    public const string COVERS_FOLDER = "covers";
    public const string SPINE_SUFFIX = "_";

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

        string animFolderPath = Path.Combine("verbs", "anim");
        while (true)
        {
            var s= GetSprite(animFolderPath, verbId + "_" + i, false);
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

    public static Sprite GetSpriteForSpine(string imageName)
    {
        return GetSprite(COVERS_FOLDER, imageName + SPINE_SUFFIX);
    }

    public static Sprite GetSpriteForFrontCover(string imageName)
    {
        return GetSprite(COVERS_FOLDER, imageName);
    }


    public static Sprite GetSpriteForAspectInStatusBar(string imageName)
    {
        return GetSprite("statusbaricons", imageName);
    }

    public static Sprite GetSpriteForElement(string imageName, int animFrame) {

        //This doesn't look for the placeholder image: this is intentional (we don't want a flickering pink question mark)
        //but might be a good way to spot missing animations
        string animFolderPath = Path.Combine("elements", "anim");
        return GetSprite(animFolderPath, imageName + "_" + animFrame, false);
    }

    public static List<Sprite> GetAnimFramesForElement(string imageName)
    {
        if (_cachedElementFrames.ContainsKey(imageName))
            return _cachedElementFrames[imageName];


        List<Sprite> frames = new List<Sprite>();

        int i = 0;
        string animFolderPath = Path.Combine("elements", "anim");
        while (true)
        {
            var s = GetSprite(animFolderPath, imageName + "_" + i, false);
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
        //hardcoded to the books back at the moment
        return GetSprite("cardbacks", "books");
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
            Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY),
        false);

    }


    public static IEnumerable<AudioClip> GetBackgroundMusic()
    {
        var musicFolderNameInResources =
            Watchman.Get<Config>().GetConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY);
        return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/background");
    }

    public static IEnumerable<AudioClip> GetImpendingDoomMusic()
    {
        var musicFolderNameInResources =
            Watchman.Get<Config>().GetConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY);
        return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/impendingdoom");
    }

    public static IEnumerable<AudioClip> GetOtherworldMusic() {
        var musicFolderNameInResources =
            Watchman.Get<Config>().GetConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY);
        return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/otherworld");
    }

    public static IEnumerable<AudioClip> GetEndingMusic(EndingFlavour endingFlavour)
    {
        var musicFolderNameInResources =
            Watchman.Get<Config>().GetConfigValue(NoonConstants.MUSIC_FOLDER_NAME_KEY);

        if (endingFlavour == EndingFlavour.Grand)
            return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/endings/grand");

        if (endingFlavour == EndingFlavour.Melancholy)
            return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/endings/melancholy");

        if (endingFlavour == EndingFlavour.Pale)
            return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/endings/melancholy");


        if (endingFlavour == EndingFlavour.Vile)
            return Resources.LoadAll<AudioClip>($"{musicFolderNameInResources}/endings/melancholy");
        else
            return null;
    }
    
    public static Sprite GetSprite(string folder, string file, bool withPlaceholder = true)
    {

        var imagesFolderNameInResources =
            Watchman.Get<Config>().GetConfigValue(NoonConstants.IMAGES_FOLDER_NAME_KEY);
        var spritePath = Path.Combine(imagesFolderNameInResources,
            folder, file);

        // Try to find the image in a mod first, in case it overrides an existing one
        var modManager = Watchman.Get<ModManager>();
        var modSprite = modManager.GetSprite(spritePath);
        if (modSprite != null)
        {
            return modSprite;
        }

        // Try to load the image from the packed resources next, and show the placeholder if not found
        var sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null || !withPlaceholder)
            return sprite;

        if (string.IsNullOrEmpty(file))
            file = PLACEHOLDER_IMAGE_NAME; //caters for cases where an empty string is passed by mistake

        return Resources.Load<Sprite>(spritePath.Replace(file, PLACEHOLDER_IMAGE_NAME));
    }


    public static Sprite GetSpriteLocalised(string imagesSubFolder, string file, string cultureId, bool withPlaceholder = true)
    {
        Sprite spriteToReturn=null;

        if (cultureId != NoonConstants.DEFAULT_CULTURE_ID)
        {
            var locFolderForCulture = NoonConstants.LOC_FOLDER_TEMPLATE.Replace(NoonConstants.LOC_TOKEN, cultureId);
            var topLevelImagesFolder = Watchman.Get<Config>().GetConfigValue(NoonConstants.IMAGES_FOLDER_NAME_KEY);
            var spritePath = Path.Combine(topLevelImagesFolder, imagesSubFolder, locFolderForCulture, file);

            // Try to find the image in a mod first, in case it overrides an existing one
            var modManager = Watchman.Get<ModManager>();
            spriteToReturn = modManager.GetSprite(spritePath);
            if (spriteToReturn != null)
            {
                return spriteToReturn;
            }

            // Try to load the image from the packed resources next. Never show the placeholder: we'll fall back to core-loc image if appropriate
            spriteToReturn = Resources.Load<Sprite>(spritePath);
        }

        if (spriteToReturn == null)
            return GetSprite(imagesSubFolder, file, withPlaceholder);
        else
            return spriteToReturn;

    }
}

