using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TimerPanel : BoardMonoBehaviour,IRecipeSituationSubscriber
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;

    public RecipeSituation RecipeSituation;


    public void ReceiveSituationUpdate(Recipe recipe, RecipeTimerState state, float timeRemaining)
    {
        if (state == RecipeTimerState.Extinct)
            DestroyObject(gameObject);
        else
        {
        float fillAmount = timeRemaining / recipe.Warmup;
        imgTimer.fillAmount = fillAmount;
        txtTimer.text = "[" + timeRemaining + "] " + recipe.Label;
        }
    }
}



    
