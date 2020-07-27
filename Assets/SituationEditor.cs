using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SituationEditor : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField] private Button btnChangeRecipe;
    [SerializeField] private Button btnForwardTen;
    [SerializeField] private Button btnBackTen;
#pragma warning restore 649

    private SituationController _situationController;

    public void Awake()
    {
        btnPlusOne.onClick.AddListener(() => AddCard(input.text));
        btnMinusOne.onClick.AddListener(() => RemoveCard(input.text));
        btnChangeRecipe.onClick.AddListener(()=>OverrideRecipe(input.text));
        btnForwardTen.onClick.AddListener(() =>Forward(10));
        btnBackTen.onClick.AddListener(() => Back(10));
    }

    public void Initialise(SituationController sc)
    {
        _situationController = sc;
    }

    public void AddCard(string elementId)
    {
        if (CheckForIssues(elementId)) return;
        _situationController.ModifyStoredElementStack(elementId, 1, new Context(Context.ActionSource.Debug));
    }
    
    public void RemoveCard(string elementId)
    {
        if (CheckForIssues(elementId)) return;
        _situationController.ModifyStoredElementStack(elementId, -1, new Context(Context.ActionSource.Debug));
    }

    public void OverrideRecipe(string recipeId)
    {
        _situationController.OverrideCurrentRecipe(recipeId);
    }

    private bool CheckForIssues(string elementId)
    {
        var element = Registry.Retrieve<ICompendium>().GetEntityById<Element>(elementId);

        if (element == null)
        {
            NoonUtility.Log("No Element with ID " + elementId + " found!");
            return true;
        }

        if (_situationController.SituationClock.State != SituationState.Ongoing)
        {
            NoonUtility.Log("Can't add cards to storage for a situation unless it's ongoing");
            return true;
        }

        return false;
    }

    public void Forward(float seconds)
    {
        _situationController.ExecuteHeartbeat(seconds);
    }

    public void Back(float seconds)
    {
        _situationController.ExecuteHeartbeat(-seconds);
    }

}
