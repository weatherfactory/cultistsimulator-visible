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

public class SituationResults : AbstractTokenContainer {

    public CanvasGroupFader canvasGroupFader;
    [SerializeField] SituationResultsPositioning cardPos;
    [SerializeField] TextMeshProUGUI dumpResultsButtonText;

    const string buttonClearResultsDefault = "Collect All";
    const string buttonClearResultsNone = "Accept";

    private SituationController controller;

    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return false; } }

    public override void Initialise() {
        throw new NotImplementedException(); // We have a separate init function here.
    }

    public void Initialise(SituationController sc) {
        controller = sc;
        _elementStacksManager = new ElementStacksManager(this, "situationresults");
    }

    public void DoReset() {
        // TODO: Clear out the cards that are still here?
    }

    public void SetOutput(List<IElementStack> allStacksToOutput) {
        if (allStacksToOutput.Any() == false)
            return;

        GetElementStacksManager().AcceptStacks(allStacksToOutput, new Context(Context.ActionSource.SituationResults));

        //currently, if the first stack is fresh, we'll turn it over anyway. I think that's OK for now.
        //cardPos.ReorderCards(allStacksToOutput);
        // we noew reorder on DisplayHere
    }

    public override void DisplayHere(IElementStack stack, Context context) {
        base.DisplayHere(stack, context);
        cardPos.ReorderCards(_elementStacksManager.GetStacks());
    }

    public override void SignalStackRemoved(ElementStackToken elementStackToken, Context context) {
        // Did we just drop the last available token? 
        // Update the badge, then reorder cards?
        controller.UpdateTokenResultsCountBadge();
        UpdateDumpButtonText();

        bool cardsRemaining = false;
        IEnumerable<IElementStack> stacks = GetOutputStacks();

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

        if (!cardsRemaining)
            controller.ResetSituation();
        else
            cardPos.ReorderCards(stacks);
    }

    public IEnumerable<IElementStack> GetOutputStacks() {
        return GetElementStacksManager().GetStacks();
    }

    public override string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }

    public void UpdateDumpButtonText() {
        if (GetOutputStacks().Count() > 0)
            dumpResultsButtonText.text = buttonClearResultsDefault;
        else
            dumpResultsButtonText.text = buttonClearResultsNone;
    }
    
}
