#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// The contents of Output spheres can be picked up by the player, but not replaced. They become unavailable once empty.
/// </summary>
[IsEmulousEncaustable(typeof(Sphere))]
public class OutputSphere : Sphere{

    [SerializeField] SituationResultsPositioning outputPositioning;

    public override SphereCategory SphereCategory => SphereCategory.Output;
    public override bool EmphasiseContents => true; //so that books show up in their final form

    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return false; } }
    public override float TokenHeartbeatIntervalMultiplier => 1;
    public bool AlwaysShroudIncomingTokens;
    

    public override void DisplayAndPositionHere(Token token, Context context) {
        base.DisplayAndPositionHere(token, context);

       outputPositioning.ArrangeTokens(GetElementTokens());
    }

    public override void AcceptToken(Token token, Context context)
    {
        if(!token.Shrouded() && AlwaysShroudIncomingTokens)
            token.Shroud(true);
        base.AcceptToken(token, context);
    }

    public override void RemoveToken(Token token,Context context) {
        // Did we just drop the last available token? 
        // Update the badge, then reorder cards?
        
        bool cardsRemaining = false;
        IEnumerable<Token> stacks = GetElementTokens();

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

        
        if (cardsRemaining)
            outputPositioning.ArrangeTokens(stacks);

        base.RemoveToken(token,context);
    }


    public override void EvictToken(Token token, Context context)
    {

        base.EvictToken(token, context);
        

    }
}
