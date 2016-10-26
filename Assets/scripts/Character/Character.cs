using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


   public class Character:IElementsContainer
    {
        public string Name { get; set; }
        private Dictionary<string, int> _elements;
        private List<IElementQuantityDisplay> _displaySubscribers;
     

        public Character()
        {
            _elements=new Dictionary<string, int>();
        _displaySubscribers=new List<IElementQuantityDisplay>();

        }

        public void SubscribeElementQuantityDisplay(IElementQuantityDisplay elementQuantityDisplay)
        {
        if(!_displaySubscribers.Contains(elementQuantityDisplay))
            _displaySubscribers.Add(elementQuantityDisplay);
        }

        public void NotifySubscribers(string elementId, int quantity)
        {
            foreach (var elementQuantityDisplay in _displaySubscribers)
            {
                elementQuantityDisplay.UpdateForElementQuantity(elementId,quantity);
            }
        }

       public void ModifyElementQuantity(string elementId, int quantity)
        {
            if (!_elements.ContainsKey(elementId))
                _elements.Add(elementId, quantity);
            else
                _elements[elementId] = _elements[elementId] + quantity;
            NotifySubscribers(elementId,quantity);
        }

        public int GetCurrentElementQuantity(string elementId)
        {
            if (!_elements.ContainsKey(elementId))
                return 0;
            return Convert.ToInt32(_elements[elementId]);
        }

        public Dictionary<string, int> GetAllCurrentElements()
        {
            return _elements;
        }
    }

