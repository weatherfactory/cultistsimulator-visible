using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerPanel : BoardMonoBehaviour
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;
    public float TimeRemaining { get { return RecipeTimer.TimeRemaining; } }

    public RecipeTimer RecipeTimer;

    private void UpdateTimerText()
    {
        txtTimer.text = "[" + RecipeTimer.TimeRemaining + "] " + RecipeTimer.Recipe.Label;
    }

    public void StartTimer(Recipe r,float? timeRemaining)
    {
        if (RecipeTimer!=null)
            throw new ApplicationException("already running a recipe (" + RecipeTimer.Recipe.Id + ")");
        RecipeTimer = new RecipeTimer(r, timeRemaining);
           
        UpdateTimerText();

    }

    public void DoHeartbeat()
    {

        imgTimer.fillAmount = RecipeTimer.TimeRemaining / RecipeTimer.Recipe.Warmup;
        RecipeTimerState timerState = RecipeTimer.DoHeartbeat();
        if (timerState==RecipeTimerState.Complete)
        { 
            BM.ExecuteRecipe(RecipeTimer.Recipe);
            BM.ExileToLimboThenDestroy(gameObject);
            
        }

        UpdateTimerText();

    }

}



public enum RecipeTimerState{ Ongoing=1,Complete=10}
    
