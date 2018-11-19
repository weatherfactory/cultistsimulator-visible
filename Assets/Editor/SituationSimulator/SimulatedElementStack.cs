using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.Logic;

namespace Assets.Editor
{
    public class SimulatedElementStack : IElementStack
    {
        public string EntityId
        {
            get { return _element == null ? null : _element.Id; }
        }
        public string SaveLocationInfo { get; set; }
        public int Quantity
        {
            get { return Defunct ? 0 : _quantity; }
        }
        public bool Defunct
        {
            get { return false; }
        }
        public bool MarkedForConsumption { get; set; }
        public bool Decays {
            get { return _element.Lifetime > 0; }
        }
        public Source StackSource { get; set; }
        public float LifetimeRemaining { get; set; }
        public IlluminateLibrarian IlluminateLibrarian { get; set; }
        public bool Unique
        {
            get { return _element != null && _element.Unique; }
        }
        public string UniquenessGroup
        {
            get { return _element == null ? null : _element.UniquenessGroup; }
        }
        public string Icon
        {
            get { return _element == null ? null : _element.Icon; }
        }

        private IElementStacksManager _currentStacksManager;
        private Dictionary<string, int> _currentMutations;
        private Element _element;
        private int _quantity;

        public IAspectsDictionary GetAspects(bool includingSelf = true)
        {
            IAspectsDictionary aspectsToReturn=new AspectsDictionary();

            if (includingSelf)
            {
                aspectsToReturn.CombineAspects(_element.AspectsIncludingSelf);
                aspectsToReturn[_element.Id] = aspectsToReturn[_element.Id] * Quantity;
            }
            else
                aspectsToReturn.CombineAspects(_element.Aspects);

            aspectsToReturn.ApplyMutations(_currentMutations);
            return aspectsToReturn;
        }

        public Dictionary<string, int> GetCurrentMutations()
        {
            return new Dictionary<string, int>(_currentMutations);
        }

        public void SetMutation(string aspectId, int value, bool additive)
        {
            if (_currentMutations.ContainsKey(aspectId))
            {
                if (additive)
                    _currentMutations[aspectId] += value;
                else
                    _currentMutations[aspectId] = value;

                if (_currentMutations[aspectId] == 0)
                    _currentMutations.Remove(aspectId);
            }
            else
                _currentMutations.Add(aspectId,value);
        }

        public Dictionary<string, string> GetXTriggers()
        {
            return _element.XTriggers;
        }

        public void ModifyQuantity(int change)
        {
            SetQuantity(_quantity + change);
        }

        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0) {
                Retire(true);
                return;
            }

            if (quantity > 1 && (Unique || !string.IsNullOrEmpty(UniquenessGroup)))
                _quantity = 1;
        }

        public void Populate(string elementId, int quantity, Source source)
        {
            Populate(Registry.Retrieve<ICompendium>().GetElementById(elementId), quantity, source);
        }

        public void Populate(Element element, int quantity, Source source)
        {
            _element = element;

            InitialiseIfStackIsNew();

            IGameEntityStorage character = Registry.Retrieve<Character>();
            var dealer = new Dealer(character);
            if (_element.Unique)
                dealer.IndicateUniqueCardManifested(_element.Id);
            if (!string.IsNullOrEmpty(_element.UniquenessGroup))
                dealer.RemoveFromAllDecksIfInUniquenessGroup(_element.UniquenessGroup);

            SetQuantity(quantity);
            LifetimeRemaining = _element.Lifetime;
            MarkedForConsumption = false;
            StackSource = source;
        }

        public void SetStackManager(IElementStacksManager manager)
        {
            var oldStacksManager = _currentStacksManager;
            _currentStacksManager = manager;
            if (oldStacksManager != null)
                oldStacksManager.RemoveStack(this);
        }

        public List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb)
        {
            return _element.ChildSlotSpecifications.Where(cs=>cs.ForVerb==forVerb || cs.ForVerb==string.Empty).ToList();
        }

        public bool HasChildSlotsForVerb(string forVerb)
        {
            return _element.HasChildSlotsForVerb(forVerb);
        }

        public IElementStack SplitAllButNCardsToNewStack(int n, Context context)
        {
            return null;
        }

        public bool AllowsIncomingMerge()
        {
            return false;
        }

        public bool AllowsOutgoingMerge()
        {
            return false;
        }

        public bool Retire(bool withVfx)
        {
            return Retire(null);
        }

        public bool Retire(string vfxName)
        {
            SetStackManager(null);
            return true;
        }

        public void Decay(float interval)
        {
        }

        public bool IsFront()
        {
            return false;
        }

        public bool CanAnimate()
        {
            return false;
        }

        public void StartArtAnimation()
        {
        }

        public void FlipToFaceUp(bool instant)
        {
        }

        public void FlipToFaceDown(bool instant)
        {
        }

        public void Flip(bool state, bool instant)
        {
        }

        public void ShowGlow(bool glowState, bool instant)
        {
        }

        public Dictionary<string, string> GetCurrentIlluminations()
        {
            return new Dictionary<string, string>();
        }

        private void InitialiseIfStackIsNew()
        {
            if (_currentMutations == null)
                _currentMutations = new Dictionary<string, int>();
            if (_currentStacksManager == null)
                _currentStacksManager = Registry.Retrieve<SimulatedTokenContainer>().GetElementStacksManager();
        }
    }
}
