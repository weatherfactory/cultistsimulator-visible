using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.Security.Cryptography;

public class AspectsDisplay : BoardMonoBehaviour
{

    [SerializeField]private GameObject prefabAspectFrame;

    private AspectFrame GetAspectFrameForId(string aspectId)
    {
        return
            GetComponentsInChildren<AspectFrame>().SingleOrDefault(a => a.Aspect.Id == aspectId);
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

    public void ChangeAspectQuantityInFrame(string aspectId, int quantity)
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
            allAspects.Add(a.Aspect.Id,a.Quantity);
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
            BM.ExileToLimboThenDestroy(transform.GetChild(i).gameObject);
        }
    }

    public void UpdateAspects(Workspace workspace)
    {

        ResetAspects();
        DraggableElementToken[] elementContainers = workspace.GetCurrentElements();

        foreach (DraggableElementToken elementContainer in elementContainers)
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
        var recipe=ContentRepository.Instance.RecipeCompendium.GetFirstRecipeForAspectsWithVerb(AllCurrentAspects(),BM.GetCurrentVerbId());
     
       BM.DisplayRecipe(recipe);
        
    }


}
