using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public interface IElementsContainer
    {
        void ModifyElementQuantity(string elementId, int quantity);
        int GetCurrentElementQuantity(string elementId);
        Dictionary<string, int> GetAllCurrentElements();
    }
