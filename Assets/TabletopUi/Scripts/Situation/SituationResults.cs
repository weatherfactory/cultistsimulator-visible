using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;

public class SituationResults : MonoBehaviour
{
    [SerializeField] private Transform _outputNotesContainer;
    [SerializeField] private OutputCardContainer _outputCardContainer;
    [SerializeField] private SituationWindow situationWindow;
    

    public void SetOutput(IEnumerable<IElementStack> stacks,INotification notification) {
        gameObject.SetActive(true);
        var newNote=PrefabFactory.CreateLocally<SituationOutputNote>(_outputNotesContainer);
        newNote.transform.localPosition = Vector2.zero;
        newNote.Initialise(notification);

        _outputCardContainer.GetElementStacksManager().AcceptStacks(stacks);

        

    }

    public void Reset()
    {
        foreach(var o in _outputNotesContainer.GetComponentsInChildren<SituationOutputNote>())
            Destroy(o.gameObject); //clear the note, ie the text results

    }

    public IEnumerable<IElementStack> GetOutputCards()
    {
        return _outputCardContainer.GetElementStacksManager().GetStacks();

    }

    public IEnumerable<ISituationOutputNote> GetOutputNotes()
    {
        return _outputNotesContainer.GetComponentsInChildren<SituationOutputNote>();

    }

}
