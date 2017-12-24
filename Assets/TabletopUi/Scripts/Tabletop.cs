using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class Tabletop : MonoBehaviour, IContainsTokens {
    private ElementStacksManager _stacksManager;


    public void ElementStackRemovedFromContainer(ElementStackToken elementStackToken)
    {
        elementStackToken.lastTablePos = elementStackToken.transform.position;
    }


    public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //we're starting with the assumption that we don't want to attempt a merge if both tokens are elementstacks; that should be catered for elsewhere

        incumbent.transform.localPosition += transform.right * incumbent.RectTransform.rect.width * 1.3f;
        incumbentMoved = true;
        DisplaySituationTokenOnTable(potentialUsurper);
    }

    public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //we're starting with the assumption that we don't want to attempt a merge if both tokens are elementstacks; that should be catered for elsewhere

        incumbent.transform.localPosition += transform.right * incumbent.RectTransform.rect.width * 1.3f;
        incumbentMoved = true;
        PutOnTable(potentialUsurper);
    }


    public ISituationAnchor CreateSituation(SituationCreationCommand creationCommand, string locatorInfo = null) {
        return Registry.Retrieve<SituationBuilder>().CreateTokenWithAttachedControllerAndSituation(creationCommand, locatorInfo);
    }

    public void DisplaySituationTokenOnTable(SituationToken token) {

        GetTokenTransformWrapper().DisplayHere(token);

        token.DisplayOnTable();
    }

    public void PutOnTable(ElementStackToken token)
    {
        _stacksManager.AcceptStack(token);

        token.DisplayOnTable();
    }

    public bool AllowDrag { get { return true; } }
    public bool AllowStackMerge { get { return true; } }


    public ElementStacksManager GetElementStacksManager() {
        //In some places we've done it Initialise. Here, we're testing if it's null and then assigning on the fly
        //This is because I'm going through and refactoring. Perhaps it should be consistent YOU TELL ME it's likely to get refactored further anyhoo
        if (_stacksManager == null)
        {
            _stacksManager = new ElementStacksManager(GetTokenTransformWrapper(),"tabletop");
        }
        return _stacksManager;
    }

    public ITokenPhysicalLocation GetTokenTransformWrapper() {
        return new TabletopTokenTransformWrapper(transform);
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }
}
