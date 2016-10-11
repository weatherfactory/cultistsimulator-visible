using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RecipeDisplay : BoardMonoBehaviour {

    [SerializeField]private Text txtRecipe;

    public void DisplayRecipe(Recipe r)
    {
        txtRecipe.text = r.Id;
    }
}
