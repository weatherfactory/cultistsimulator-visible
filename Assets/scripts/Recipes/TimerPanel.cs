using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerPanel : MonoBehaviour
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;
    [SerializeField] public Recipe RecipeWaiting;
    public int TimeRemaining;


    public void StartTimer(Recipe forRecipe)
    {
        if(TimeRemaining>0)
            throw new ApplicationException("already running a recipe (" + RecipeWaiting.Id + ")"); 

        RecipeWaiting = forRecipe;
        txtTimer.text = forRecipe.Label;
    }

}
