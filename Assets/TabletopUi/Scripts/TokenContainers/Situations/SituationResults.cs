#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;

public class SituationResults : Sphere {

    public CanvasGroupFader canvasGroupFader;
    [SerializeField] SituationResultsPositioning cardPos;
    [SerializeField] TextMeshProUGUI dumpResultsButtonText;

    public override ContainerCategory ContainerCategory => ContainerCategory.Output;

    private string buttonClearResultsDefault;
    private string buttonClearResultsNone;

    
    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return false; } }


    public void Initialise() {
        buttonClearResultsDefault = "VERB_COLLECT";
        buttonClearResultsNone = "VERB_ACCEPT";
    }

    public void UpdateDisplay(SituationEventData eventData)
    {
        switch (eventData.SituationState)
        {
            case SituationState.Complete:
                canvasGroupFader.Show();
                break; 
            default:
                canvasGroupFader.Hide();
                break;
        }

    }


    public void SetOutput(List<ElementStackToken> allStacksToOutput) {
        if (allStacksToOutput.Any() == false)
            return;

        AcceptStacks(allStacksToOutput, new Context(Context.ActionSource.SituationResults));

        //currently, if the first stack is fresh, we'll turn it over anyway. I think that's OK for now.
        //cardPos.ReorderCards(allStacksToOutput);
        // we noew reorder on DisplayHere
    }



    public override void AcceptStack(ElementStackToken stack, Context context)
    {
        base.AcceptStack(stack,context);
        stack.Shroud(true);
    }
        //stack.Shroud(true);)

    public override void DisplayHere(ElementStackToken stack, Context context) {
        base.DisplayHere(stack, context);
        cardPos.ReorderCards(GetStacks());
    }

    public override void RemoveStack(ElementStackToken elementStackToken) {
        // Did we just drop the last available token? 
        // Update the badge, then reorder cards?

        UpdateDumpButtonText();

        bool cardsRemaining = false;
        IEnumerable<ElementStackToken> stacks = GetOutputStacks();

        // Window is open? Check if it was the last card, then reset automatically
        if (gameObject.activeInHierarchy) {
            foreach (var item in stacks) {
                if (item != null && item.Defunct == false) {
                    cardsRemaining = true;
                    break;
                }
            }
        }
        else {
            // Window is closed? ensure we never reset only reorder
            cardsRemaining = true;
        }

        //if (!cardsRemaining)
        //    controller.ResetSituation();
        //else
        //    cardPos.ReorderCards(stacks);

        if (cardsRemaining)
            cardPos.ReorderCards(stacks);
    }

    public IEnumerable<ElementStackToken> GetOutputStacks() {
        return GetStacks();
    }

    public override string GetPath()
    {
        return "output";
        //   return (token.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + token.RectTransform.localPosition.y).ToString();
    }

    public void UpdateDumpButtonText() {
        if (GetOutputStacks().Any())
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsDefault);
        else
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsNone);
    }
    
}
