using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI
{
    public class ElementStack : DraggableToken, IElementStack
    {

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject selectedMarker;
        private Element _element;
        private int _quantity;
        private IElementStacksWrapper currentWrapper;

        public string ElementId
        {
            get { return _element == null ? null : _element.Id; }
        }

        public string Label
        {
            get { return _element == null ? null : _element.Label; }
        }

        public int Quantity
        {
            get { return _quantity; }
        }

        public bool Defunct { get; private set; }


        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0)
            {
                Remove();
                return;
            }
            DisplayInfo();
        }


        public void ModifyQuantity(int change)
        {
            SetQuantity(_quantity + change);
        }

        public bool Remove()
        {
            DestroyObject(gameObject);
            if (Defunct)
                return false;
            Defunct = true;
            return true;
        }

        public void Populate(string elementId, int quantity)
        {

            _element = Registry.Compendium.GetElementById(elementId);
            SetQuantity(quantity);

            name = "Card_" + elementId;
            if (_element == null)
                return;

            DisplayInfo();
            DisplayIcon();
        }


        private void DisplayInfo()
        {
            text.text = _element.Label + "(" + Quantity + ")";
        }

        private void DisplayIcon()
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Id);
            artwork.sprite = sprite;
        }

        public IAspectsDictionary GetAspects()
        {
            return _element.AspectsIncludingSelf;
        }

        public List<SlotSpecification> GetChildSlotSpecifications()
        {
            return _element.ChildSlotSpecifications;
        }


        public bool HasChildSlots()
        {
            return _element.HasChildSlots();
        }

        public Sprite GetSprite()
        {
            return artwork.sprite;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            notifier.ShowElementDetails(this);
            base.OnPointerClick(eventData);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            ElementStack droppedCard = DraggableToken.itemBeingDragged as ElementStack;
            ElementStack droppedOnStack = this as ElementStack;
            if (droppedOnStack != null && droppedCard != null && droppedOnStack.ElementId == droppedCard.ElementId)
            {
                droppedOnStack.SetQuantity(droppedOnStack.Quantity + droppedCard.Quantity);
                DraggableToken.resetToStartPos = false;
                droppedCard.SetQuantity(0);
            }
        }

        public void SplitAllButNCardsToNewStack(int n)
        {
            if (Quantity > n)
            {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStack>(transform.parent);

                cardLeftBehind.Populate(ElementId, Quantity - n);
                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(1);
                cardLeftBehind.transform.position = transform.position;
                var gateway = container.GetElementStacksGateway();

               gateway.AcceptStack(cardLeftBehind);
            }
   
        }

        protected override void StartDrag(PointerEventData eventData)
        {

            SplitAllButNCardsToNewStack(1);

            base.StartDrag(eventData);


        }
    }
}
