using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// ElementsContainers are given to Recipes. Currently, some recipes take the Character; other recipes
/// (interactivesituations) have their own internal container
/// </summary>
    public abstract class BaseElementsContainer: IElementsContainer
    {
    protected Dictionary<string, int> _elements;
    protected List<IElementQuantityDisplay> _elementsDisplaySubscribers;
    protected List<ICharacterInfoSubscriber> _detailsSubscribers;

        public abstract void ModifyElementQuantity(string elementId, int quantityChange);
        public abstract void TriggerSpecialEvent(string endingId);


        protected BaseElementsContainer()
        {
        _elements = new Dictionary<string, int>();
        _elementsDisplaySubscribers = new List<IElementQuantityDisplay>();
        _detailsSubscribers = new List<ICharacterInfoSubscriber>();

    }

        public virtual int GetCurrentElementQuantity(string elementId)
        {
            if (!_elements.ContainsKey(elementId))
                return 0;
            return Convert.ToInt32(_elements[elementId]);
        }

        public virtual Dictionary<string, int> GetAllCurrentElements()
        {
        return _elements;
        }

        public virtual bool IsInternal()
        {
            return false;
        }

        /// <summary>
    /// </summary>
    /// <returns>Elements which have been made available for use by another actor</returns>
    public virtual Dictionary<string, int> GetOutputElements()
    {
        return _elements;
    }


    /// <summary>
    /// update the UI with the current resource quantity, and also make any necessary adjustments to workspace/dragitem display (because we've had stockpile removed)
    /// </summary>
    /// <param name="elementId"></param>
    /// <param name="quantityInstockpile"></param>
    /// <param name="workspaceAdjustment"></param>
    public void PublishElementQuantityUpdate(string elementId, int quantityInstockpile, int workspaceAdjustment)
    {
        foreach (var elementQuantityDisplay in _elementsDisplaySubscribers)
        {
            elementQuantityDisplay.ElementQuantityUpdate(elementId, quantityInstockpile, workspaceAdjustment);
        }
    }
}

