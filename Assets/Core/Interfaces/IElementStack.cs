using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Enums;

namespace Assets.Core.Interfaces
{
    public interface IElementStack
    {
        string Id { get; }
        IElementStacksManager CurrentStacksManager { get; set; }
        string SaveLocationInfo { get; set; }
        int Quantity { get; }
        bool Defunct { get; }
        bool MarkedForConsumption { get; set; }
        bool Decays { get; }
        IAspectsDictionary GetAspects(bool includingSelf = true);
        Dictionary<string, string> GetXTriggers();
        //should return false if Remove has already been called on this card
        void ModifyQuantity(int change);
        void SetQuantity(int quantity);
        void Populate(string elementId, int quantity,Source source);
        List<SlotSpecification> GetChildSlotSpecifications();
        bool HasChildSlots();
        void SplitAllButNCardsToNewStack(int n);
        bool AllowMerge();
        bool Retire(bool withVFX);
        void Decay(float interval);

        bool IsFront();

        bool CanAnimate();
        void StartArtAnimation();

        void FlipToFaceUp(bool instant);
        void FlipToFaceDown(bool instant);
        void Flip(bool state, bool instant);
        void ShowGlow(bool glowState, bool instant);

        Source StackSource { get; set; }
    }
}
