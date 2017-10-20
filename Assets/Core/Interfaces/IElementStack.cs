using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    public interface IElementStack
    {
        string Id { get; }
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
        void Populate(string elementId, int quantity);
        List<SlotSpecification> GetChildSlotSpecifications();
        bool HasChildSlots();
        void SplitAllButNCardsToNewStack(int n);
        bool AllowMerge();
        bool Retire(bool withVFX);
        void Decay(float interval);
    }
}
