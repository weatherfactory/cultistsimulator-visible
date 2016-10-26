using System;
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

    public void DoHeartbeat()
    {

        imgTimer.fillAmount = RecipeSituation.TimeRemaining / RecipeSituation.Recipe.Warmup;
        RecipeTimerState timerState = RecipeSituation.DoHeartbeat();
        if (timerState==RecipeTimerState.Complete)
        {
            List<Recipe> recipesToExecute =
                ContentRepository.Instance.RecipeCompendium.GetActualRecipesToExecute(RecipeSituation.Recipe, BM);
            foreach(Recipe r in recipesToExecute)
            r.Do(BM,BM);
            
            BM.ExileToLimboThenDestroy(gameObject);
            
        }

        UpdateTimerText();

    }

}



public enum RecipeTimerState{ Ongoing=1,Complete=10}
    
