using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class RecipeDisplay : BoardMonoBehaviour {

    [SerializeField]private TextMeshProUGUI txtRecipe;
    [SerializeField]private Button btnExecuteRecipe;
    public Recipe CurrentRecipe;
    public void DisplayRecipe(Recipe r)
    {
        if (r == null)
        {
            CurrentRecipe = null;
        txtRecipe.text = "No matching recipe...";
        btnExecuteRecipe.interactable = false;
        }
        else
        {
            CurrentRecipe = r;
            txtRecipe.text = r.Label + " (" + r.StartDescription +")";
            btnExecuteRecipe.interactable = true;
        }
    }
}
