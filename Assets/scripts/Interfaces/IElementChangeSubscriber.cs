using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public interface IElementQuantityDisplay
    {
        void ElementQuantityUpdate(string elementId, int currentQuantityInStockpile, int workspaceQuantityAdjustment);
    }

