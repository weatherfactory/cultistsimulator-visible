using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class Character:IElementsContainer
    {
    private readonly Dictionary<string, int> _elements;
    private readonly Dictionary<string, int> _elementsInWorkspace;
    private readonly List<IElementQuantityDisplay> _elementsDisplaySubscribers;
    private readonly List<ICharacterInfoSubscriber> _detailsSubscribers;
    private string _title;
    private string _firstName;
    private string _lastName;
    public CharacterState State { get; set; }
    private string _endingTriggeredId;

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

    public string EndingTriggeredId
    { get { return _endingTriggeredId; } }




        public Character()
        {
            _elements=new Dictionary<string, int>();
        _elementsInWorkspace=new Dictionary<string, int>();
        _elementsDisplaySubscribers=new List<IElementQuantityDisplay>();
        _detailsSubscribers=new List<ICharacterInfoSubscriber>();
            State = CharacterState.Viable;

        }

        public void SubscribeElementQuantityDisplay(IElementQuantityDisplay elementQuantityDisplay)
        {
        if(!_elementsDisplaySubscribers.Contains(elementQuantityDisplay))
            _elementsDisplaySubscribers.Add(elementQuantityDisplay);
        }

        public void Subscribe(ICharacterInfoSubscriber characterInfoSubscriber)
        {
            if(!_detailsSubscribers.Contains(characterInfoSubscriber))
            _detailsSubscribers.Add(characterInfoSubscriber);
        }

        public void PublishElementQuantityInResources(string elementId, int quantity)
        {
            foreach (var elementQuantityDisplay in _elementsDisplaySubscribers)
            {
                elementQuantityDisplay.UpdateElementQuantityInResources(elementId,quantity);
            }
        }

        public void NotifySubscribersOfDetailsChange()
        {
            foreach(var d in _detailsSubscribers)
                d.ReceiveUpdate(this);
            
        }

       public void ModifyElementQuantity(string elementId, int quantity)
        {
            if (!_elements.ContainsKey(elementId))
                _elements.Add(elementId, quantity);
            else
                _elements[elementId] = _elements[elementId] + quantity;
            PublishElementQuantityInResources(elementId, GetCurrentElementQuantityInResources(elementId));
        }

   public bool ElementToWorkspace(string elementId, int plusQuantity)
   {
       if (GetCurrentElementQuantity(elementId) < plusQuantity)
           return false;
        if(!_elementsInWorkspace.ContainsKey(elementId))
            _elementsInWorkspace.Add(elementId,plusQuantity);
        else
           _elementsInWorkspace[elementId] += plusQuantity;
       return true;
   }

    public bool ElementFromWorkspace(string elementId, int minusQuantity)
    {
        if (GetCurrentElementQuantityInWorkspace(elementId) < minusQuantity)
            return false;

        if (!_elementsInWorkspace.ContainsKey(elementId))
            _elementsInWorkspace.Add(elementId, -minusQuantity);
        else
            _elementsInWorkspace[elementId] -= minusQuantity;
        return true;
    }

    public int GetCurrentElementQuantityInWorkspace(string elementId)
    {
        return _elementsInWorkspace.ContainsKey(elementId) ? _elementsInWorkspace[elementId] : 0;
    }

        public int GetCurrentElementQuantityInResources(string elementId)
        {
            var inResources = _elements.ContainsKey(elementId) ? _elements[elementId] : 0;

            if (_elementsInWorkspace.ContainsKey(elementId))
                inResources -= _elementsInWorkspace[elementId];

            return inResources;
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

        public void TriggerEnding(string endingId)
        {
            State=CharacterState.Extinct;
            _endingTriggeredId = endingId;
          NotifySubscribersOfDetailsChange();
        }
    }

