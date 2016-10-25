using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public interface IElementsContainer
    {
        void ModifyElementQuantity(string elementId, int quantity);
        void ModifyElementQuantity(string elementId, int quantity, int? siblingIndex);
        int GetCurrentElementQuantity(string elementId);

    }
