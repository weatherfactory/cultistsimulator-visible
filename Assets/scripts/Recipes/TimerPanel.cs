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

    public BaseRecipeSituation BaseRecipeSituation;


    public void ReceiveSituationUpdate(SituationInfo info)
    {
        if (info.State == RecipeTimerState.Extinct)
            DestroyObject(gameObject);
        else
        {
        float fillAmount = info.TimeRemaining / info.Warmup;
        imgTimer.fillAmount = fillAmount;
        txtTimer.text = "[" + info.TimeRemaining + "] " + info.Label;

            if (info.ElementsInSituation.Count > 0)
            {
                pnlContents.ResetAspects();
                foreach(string k in info.ElementsInSituation.Keys)
                pnlContents.ChangeAspectQuantity(k,info.ElementsInSituation[k],20);
            }

        }
    }
}



    
