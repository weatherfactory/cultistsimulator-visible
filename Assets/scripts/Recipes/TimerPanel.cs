using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerPanel : MonoBehaviour
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;
    [SerializeField] public Recipe Recipe;
    public float TimeRemaining;

    private void UpdateTimerText()
    {
        txtTimer.text = "[" + TimeRemaining + "] " + Recipe.Label;
    }

    public void StartTimer(Recipe r)
    {
        if(TimeRemaining>0)
            throw new ApplicationException("already running a recipe (" + Recipe.Id + ")");

        Recipe = r;
        TimeRemaining = r.Warmup;
        UpdateTimerText();

    }

    public void DoHeartbeat()
    {
        UpdateTimerText();
        TimeRemaining--;
        
        imgTimer.fillAmount = TimeRemaining/Recipe.Warmup;
    }

}
