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


    //public void StartTimer(Recipe r,float? timeRemaining)
    //{
    //    if (RecipeSituation!=null)
    //        throw new ApplicationException("already running a recipe (" + RecipeSituation.Recipe.Id + ")");
    //    RecipeSituation = new RecipeSituation(r, timeRemaining);
    //    UpdateTimerText();

    //}

    //public RecipeTimerState DoHeartbeat(Character c)
    //{
        
    //    RecipeTimerState state=RecipeSituation.DoHeartbeat(c, ContentRepository.Instance.RecipeCompendium);
    //    if (state==RecipeTimerState.Complete)     
    //        BM.ExileToLimboThenDestroy(gameObject);
    //    else
    //    {
    //        float fillAmount = RecipeSituation.TimeRemaining / RecipeSituation.Recipe.Warmup;
    //        imgTimer.fillAmount = fillAmount;
    //        UpdateTimerText();
    //    }
    //    return state;
    //}



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



    
