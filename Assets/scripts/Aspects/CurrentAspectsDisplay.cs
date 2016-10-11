﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.Security.Cryptography;

public class CurrentAspectsDisplay : BoardMonoBehaviour
{

    [SerializeField] private GameObject objLimbo;
    [SerializeField]private GameObject prefabAspectFrame;

    private AspectFrame GetAspectFrameForId(string aspectId)
    {
        return
            GetComponentsInChildren<AspectFrame>().SingleOrDefault(a => a.AspectId == aspectId);
    }

    private void AddAspectToDisplay(string aspectId, int quantity)
    {
        GameObject newAspectDisplay = Instantiate(prefabAspectFrame, transform) as GameObject;
        if (newAspectDisplay != null)
        {
            AspectFrame aspectFrame = newAspectDisplay.GetComponent<AspectFrame>();
            aspectFrame.PopulateDisplay(aspectId, quantity, ContentRepository.Instance);
        }
    }

    private void ChangeAspectQuantityInFrame(string aspectId, int quantity)
    {
        AspectFrame existingAspect = GetAspectFrameForId(aspectId);
        if (existingAspect)
            existingAspect.ModifyQuantity(quantity);
        else
            AddAspectToDisplay(aspectId, quantity);
    }


    public Dictionary<string, int> AllCurrentAspects()
    {
        Dictionary<string, int> allAspects=new Dictionary<string, int>();
        foreach (AspectFrame a in GetComponentsInChildren<AspectFrame>())
        {
            allAspects.Add(a.AspectId,a.Quantity);
        }
        return allAspects;
    }


    public void ResetAspects()
    {
        //this drove me fucking nuts. It's my old nemesis: removing items from a collection as you go
        //but Unity lets you do it with transform
        //but the SetParent actually alters the collection
        //so foreach and standard iteration don't work. Counting down to 0 works.
        //and of course the SetParent to limbo is because Destroy happens at frame end.
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            transform.GetChild(i).SetParent(objLimbo.transform);

        }
    }

    public void UpdateAspects(IContainsElement[] elementContainers)
    {
         ResetAspects();

        foreach (IContainsElement elementContainer in elementContainers)
        {

            foreach (KeyValuePair<string, int> kvp in elementContainer.Element.Aspects)
            {
                ChangeAspectQuantityInFrame(kvp.Key, kvp.Value);
            }

        }

        DisplayRecipesForCurrentAspects();
    }


    private void DisplayRecipesForCurrentAspects()
    {
        var recipe=ContentRepository.Instance.RecipeCompendium.GetFirstRecipeForAspects(AllCurrentAspects());
       DebugLogAspects();
        
       BM.DisplayCurrentRecipe(recipe);
        
    }

    private void DebugLogAspects()
    {
        Dictionary<string, int> AllAspects = this.AllCurrentAspects();
        foreach (KeyValuePair<string, int> kv in AllAspects)
            BM.BoardLog(kv.Key + " - " + kv.Value);
    }
}
