using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TimerPanel : BoardMonoBehaviour,IRecipeSituationSubscriber
{

    [SerializeField] private Image imgTimer;
    [SerializeField] private Text txtTimer;
    [SerializeField] private AspectsDisplay pnlContents;
    [SerializeField]private GameObject elementToken;

    public RecipeSituation RecipeSituation;


    public void ReceiveSituationUpdate(Recipe recipe, RecipeTimerState state, float timeRemaining,SituationInfo info)
    {
        if (state == RecipeTimerState.Extinct)
            DestroyObject(gameObject);
        else
        {
        float fillAmount = timeRemaining / recipe.Warmup;
        imgTimer.fillAmount = fillAmount;
        txtTimer.text = "[" + timeRemaining + "] " + recipe.Label;

            if (info.Elements.Count > 0)
            {
                pnlContents.ResetAspects();
                foreach(string k in info.Elements.Keys)
                pnlContents.ChangeAspectQuantity(k,info.Elements[k]);
            }

        }
    }
}



    
