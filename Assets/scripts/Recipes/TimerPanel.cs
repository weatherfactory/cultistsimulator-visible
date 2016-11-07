using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Noon;
using UnityEngine.Assertions;
using UnityEngine.UI;


/// <summary>
/// after the UI shift, this should be combined with Workspace (and then split out into smaller classes)
/// </summary>
public class TimerPanel : BoardMonoBehaviour,IRecipeSituationSubscriber,IElementSlotEventSubscriber
{

    [SerializeField] private Image imgTimer;
    [SerializeField]
    private Text txtTimer;

    [SerializeField] private SlotsPanel pnlSlots;
    [SerializeField]
    private AspectsDisplay pnlContents;
    
    [SerializeField]
    protected GameObject elementToken;

 
    /// <summary>
    /// this whole method is a hack until we do the UI migration
    /// </summary>
    
    public void SituationBegins(SituationInfo withInfo,BaseRecipeSituation rs)
    {
        
        InteractiveRecipeSituation possibleInteractiveSituation = rs as InteractiveRecipeSituation;
        if (possibleInteractiveSituation != null)
        {
            //if we have any outstanding elements in slots, grab and consume them, then pass them back
            //post UI migration, this should be a separate end method - SituationInfo is pretty bloated
            //if we have any slotted elements, consume them and pass them back
            Dictionary<string, int> internalElementsChanges = pnlSlots.GrabAndConsumeElements();
            foreach (string k in internalElementsChanges.Keys)
                possibleInteractiveSituation.ModifyInternalElementQuantity(k, internalElementsChanges[k]);


            if (withInfo.DisplayElementsInSituation.Count > 0)
            {
                pnlContents.ResetAspects();
                foreach (string k in withInfo.DisplayElementsInSituation.Keys)
                    pnlContents.ChangeAspectQuantity(k, withInfo.DisplayElementsInSituation[k], 20);
            }

            if (withInfo.DisplayChildSlotSpecifications.Count > 0)
            {
                pnlSlots.DisplaySlotsForRecipe(withInfo.DisplayChildSlotSpecifications);
            }
        }
    }

    public virtual void SituationUpdated(SituationInfo info)
    {

        if (info.State == RecipeTimerState.Extinct)
            DestroyObject(gameObject);
        else
        {
            updateDisplay(info);
        }


    }

    private void updateDisplay(SituationInfo withInfo)
    {
        float fillAmount = withInfo.TimeRemaining / withInfo.Warmup;
        imgTimer.fillAmount = fillAmount;
        txtTimer.text = "[" + withInfo.TimeRemaining + "] " + withInfo.Label;

    }

    public void ElementAddedToSlot(Element element, SlotReceiveElement slot)
    {

//nothing right now, actually
    }

    public void ElementCannotBeAddedToSlot(Element element, ElementSlotMatch match)
    {
        if (match.ElementSlotSuitability == ElementSlotSuitability.ForbiddenAspectPresent)
        {
            string problemAspects = NoonUtility.ProblemAspectsDescription(match);
            BM.Log("Elements with the " + problemAspects + " aspects are unacceptable here. *Unacceptable*.", Style.Assertive);

            return;
        }

        if (match.ElementSlotSuitability == ElementSlotSuitability.RequiredAspectMissing)
        {
            string problemAspects = NoonUtility.ProblemAspectsDescription(match);
            BM.Log("Only elements with the " + problemAspects + " aspects can go here.", Style.Assertive);

            return;
        }
    }
}



    
