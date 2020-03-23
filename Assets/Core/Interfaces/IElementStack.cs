﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Enums;
using UnityEngine;

namespace Assets.Core.Interfaces
{
    public interface IElementStack
    {
        string EntityId { get; }
        
        string SaveLocationInfo { get; set; }
        int Quantity { get; }
        bool Defunct { get; }
        bool MarkedForConsumption { get; set; }
        bool Decays { get; }
		Vector2? LastTablePos { get; set; }
        IAspectsDictionary GetAspects(bool includingSelf = true);
        Dictionary<string,int> GetCurrentMutations();
        void SetMutation(string aspectId, int value,bool additive);
        Dictionary<string, List<MorphDetails>> GetXTriggers();
        //should return false if Remove has already been called on this card
        void ModifyQuantity(int change,Context context);
        void SetQuantity(int quantity, Context context);
        void Populate(string elementId, int quantity,Source source);
        void SetStackManager(IElementStacksManager manager);
        List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb);
        bool HasChildSlotsForVerb(string forVerb);
        IElementStack SplitAllButNCardsToNewStack(int n, Context context);
        bool AllowsIncomingMerge();	// Can cards be merged onto this stack?
		bool AllowsOutgoingMerge();	// Can this stack be merged onto another stack?
        bool Retire(CardVFX vfxName);
        void Decay(float interval);

        bool IsFront();

        bool CanAnimate();
        void StartArtAnimation();

        void FlipToFaceUp(bool instant);
        void FlipToFaceDown(bool instant);
        void Flip(bool state, bool instant);
        void ShowGlow(bool glowState, bool instant);

        Source StackSource { get; set; }
        float LifetimeRemaining { get; set; }
        IlluminateLibrarian IlluminateLibrarian { get; set; }
        bool Unique { get; }
        string UniquenessGroup { get; }
        string Icon { get; }
        Dictionary<string, string> GetCurrentIlluminations();
    }
}
