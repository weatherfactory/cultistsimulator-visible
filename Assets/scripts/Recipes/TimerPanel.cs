using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts;
using UnityEngine.UI;

public class TimerPanel : BoardMonoBehaviour,IRecipeSituationSubscriber
{

    [SerializeField] private Image imgTimer;
    [SerializeField]
    private Text txtTimer;

    [SerializeField] private SlotsPanel pnlSlots;
    [SerializeField]
    private AspectsDisplay pnlContents;
    
    [SerializeField]
    protected GameObject elementToken;

    public BaseRecipeSituation BaseRecipeSituation;


    public void SituationBegins(SituationInfo withInfo)
    {

        if (withInfo.DisplayElementsInSituation.Count > 0)
        {
            pnlContents.ResetAspects();
           // foreach (string k in withInfo.DisplayElementsInSituation.Keys)
              //  pnlContents.ChangeAspectQuantity(k, withInfo.DisplayElementsInSituation[k]);
        }

        if (withInfo.DisplayChildSlotSpecifications.Count > 0)
        {
            pnlSlots.DisplaySlotsForRecipe(withInfo.DisplayChildSlotSpecifications);
        }
    }

    public virtual void SituationUpdated(SituationInfo info)
    {
        if (info.State == RecipeTimerState.Extinct)
            DestroyObject(gameObject);
        else
        {
            populateDisplay(info);
        }


    }

    private void populateDisplay(SituationInfo withInfo)
    {
        float fillAmount = withInfo.TimeRemaining / withInfo.Warmup;
        imgTimer.fillAmount = fillAmount;
        txtTimer.text = "[" + withInfo.TimeRemaining + "] " + withInfo.Label;

    }
}



    
