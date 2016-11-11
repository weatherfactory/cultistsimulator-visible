using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public interface IElementsContainer
    {
        void ModifyElementQuantity(string elementId, int quantityChange);
        int GetCurrentElementQuantity(string elementId);
        Dictionary<string, int> GetAllCurrentElements();
        void TriggerSpecialEvent(string endingId);
        void PublishElementQuantityUpdate(string elementId, int quantityInstockpile, int workspaceAdjustment);
    /// <summary>
    /// does this container live in something else, like a situation?
    /// </summary>
        bool IsInternal();

        /// <summary>
        /// </summary>
        /// <returns>Elements which have been made available for use by another actor</returns>
        Dictionary<string, int> GetOutputElements();
    }
