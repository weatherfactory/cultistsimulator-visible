using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RecipeDisplay : BoardMonoBehaviour {

    [SerializeField]private Text txtRecipe;

    public void DisplayRecipe(Recipe r)
    {
        if (r == null)
            txtRecipe.text = "No matching recipe...";
        else

            txtRecipe.text = r.Label;
    }
}
