﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TimerPanel : BoardMonoBehaviour
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;
    public float TimeRemaining { get { return RecipeSituation.TimeRemaining; } }

    public RecipeSituation RecipeSituation;

    private void UpdateTimerText()
    {
        txtTimer.text = "[" + RecipeSituation.TimeRemaining + "] " + RecipeSituation.Recipe.Label;
    }

    public void StartTimer(Recipe r,float? timeRemaining)
    {
        if (RecipeSituation!=null)
            throw new ApplicationException("already running a recipe (" + RecipeSituation.Recipe.Id + ")");
        RecipeSituation = new RecipeSituation(r, timeRemaining);
           
        UpdateTimerText();

    }

    public RecipeTimerState DoHeartbeat(Character c)
    {
        
        RecipeTimerState state=RecipeSituation.DoHeartbeat(BM, c, ContentRepository.Instance.RecipeCompendium);
        if (state==RecipeTimerState.Complete)     
            BM.ExileToLimboThenDestroy(gameObject);
        else
        {
            float fillAmount = RecipeSituation.TimeRemaining / RecipeSituation.Recipe.Warmup;
            imgTimer.fillAmount = fillAmount;
            UpdateTimerText();
        }
        return state;
    }

}



    
