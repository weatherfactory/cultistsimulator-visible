using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Interfaces
{
    public interface IElementStack
    {
        string Id { get; }
        string LocationInfo { get; }
        int Quantity { get; }
        bool Defunct { get; }
        IAspectsDictionary GetAspects();
        //should return false if Remove has already been called on this card
        void ModifyQuantity(int change);
        void SetQuantity(int quantity);
        void Populate(string elementId, int quantity);
        List<SlotSpecification> GetChildSlotSpecifications();
        bool HasChildSlots();
        void SplitAllButNCardsToNewStack(int n);
    }
}
