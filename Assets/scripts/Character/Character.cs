using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class Character:IElementsContainer
    {
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifySubscribersOfDetailsChange();
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
            NotifySubscribersOfDetailsChange();
        }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
            NotifySubscribersOfDetailsChange();
        }
        }

        private Dictionary<string, int> _elements;
        private List<IElementQuantityDisplay> _elementsDisplaySubscribers;
        private List<ICharacterDetailsDisplay> _detailsSubscribers;
        private string _title;
        private string _firstName;
        private string _lastName;


        public Character()
        {
            _elements=new Dictionary<string, int>();
        _elementsDisplaySubscribers=new List<IElementQuantityDisplay>();
        _detailsSubscribers=new List<ICharacterDetailsDisplay>();

        }

        public void SubscribeElementQuantityDisplay(IElementQuantityDisplay elementQuantityDisplay)
        {
        if(!_elementsDisplaySubscribers.Contains(elementQuantityDisplay))
            _elementsDisplaySubscribers.Add(elementQuantityDisplay);
        }

        public void SubscribeDetailsDisplay(ICharacterDetailsDisplay characterDetailsDisplay)
        {
            if(!_detailsSubscribers.Contains(characterDetailsDisplay))
            _detailsSubscribers.Add(characterDetailsDisplay);
        }

        public void NotifySubscribersOfElementQuantityChange(string elementId, int quantity)
        {
            foreach (var elementQuantityDisplay in _elementsDisplaySubscribers)
            {
                elementQuantityDisplay.UpdateForElementQuantity(elementId,quantity);
            }
        }

        public void NotifySubscribersOfDetailsChange()
        {
            foreach(var d in _detailsSubscribers)
                d.UpdateDisplay(this);
            
        }

       public void ModifyElementQuantity(string elementId, int quantity)
        {
            if (!_elements.ContainsKey(elementId))
                _elements.Add(elementId, quantity);
            else
                _elements[elementId] = _elements[elementId] + quantity;
            NotifySubscribersOfElementQuantityChange(elementId, _elements[elementId]);
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

