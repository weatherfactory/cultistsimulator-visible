using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class KnownRecipeDisplay : BoardMonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI txtRecipe;
    [SerializeField]
    private Text txtFound;
    private int displayedRecipeIndex;
    private Recipe displayedRecipe;
    private List<Recipe> currentMatchedKnownRecipes=new List<Recipe>();
    [SerializeField]private VerbNeeded pnlVerbNeeded;
    [SerializeField] private AspectsDisplay pnlAspectsNeeded;


    public void MatchKnownRecipes(string textToMatch)
    {
        currentMatchedKnownRecipes = BM.GetKnownRecipes(textToMatch);
        if (currentMatchedKnownRecipes.Count == 0)
        { 
            txtRecipe.text = "Nothing comes to mind -";
            txtFound.text = "";
        }
        else
        {

            //if the current recipe is in the matched list, update to show that recipe at whatever index
            if(displayedRecipe!=null && currentMatchedKnownRecipes.Any(r=>r.Id==displayedRecipe.Id))
                SetDisplayIndex(currentMatchedKnownRecipes.FindIndex(r=>r.Id==displayedRecipe.Id));
            //otherwise, just show the first index in the list
            SetDisplayIndex(0);
        }


    }

    public void SetDisplayIndex(int newIndex)
    {
        if (newIndex < 0 || newIndex >= currentMatchedKnownRecipes.Count)
        {
            pnlAspectsNeeded.ResetAspects(); 
            return;
        }
        displayedRecipeIndex = newIndex;
        txtFound.text = (displayedRecipeIndex + 1)+ " of " + currentMatchedKnownRecipes.Count;
        displayedRecipe = currentMatchedKnownRecipes[displayedRecipeIndex];
        txtRecipe.text = "<b>" + displayedRecipe.Label + "</b>\n" + displayedRecipe.Description;
        pnlAspectsNeeded.ResetAspects();
        foreach (KeyValuePair<string,int> reqKeyValuePair in displayedRecipe.Requirements)
        {
            pnlAspectsNeeded.ChangeAspectQuantityInFrame(reqKeyValuePair.Key,reqKeyValuePair.Value);
        }
        
        pnlVerbNeeded.SetVerb(displayedRecipe.ActionId);
    }

    public void Forward()
    {
        if(currentMatchedKnownRecipes.Count>0 && (displayedRecipeIndex+1)<currentMatchedKnownRecipes.Count)
            SetDisplayIndex(displayedRecipeIndex+1);
    }
    public void Back()
    {
        if (currentMatchedKnownRecipes.Count > 0 && displayedRecipeIndex>0)
            SetDisplayIndex(displayedRecipeIndex - 1);
    }

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
