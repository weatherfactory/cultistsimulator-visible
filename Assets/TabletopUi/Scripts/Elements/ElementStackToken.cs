using System;
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
    public class ElementStackToken : DraggableToken, IElementStack, IGlowableView
    {

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GraphicFader glowImage;
        [SerializeField] GameObject stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [SerializeField] CardBurnEffect cardBurnFX;

        private Element _element;
        private int _quantity;
        private ITokenTransformWrapper currentWrapper;
        private float lifetimeRemaining;


        public override string Id
        {
            get { return _element == null ? null : _element.Id; }
        }

        public bool Decays
        {
            get { return _element.Lifetime > 0; }
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
        public bool MarkedForConsumption { get; set; }


        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0)
            {
                Retire(true);
                return;
            }
            DisplayInfo();
        }


        public void ModifyQuantity(int change)
        {
            SetQuantity(_quantity + change);
        }

        public override bool Retire()
        {
            return Retire(true);
        }

        public bool Retire(bool withFlameEffect)
        {
            if (Defunct)
                return false;

            Defunct = true;

            if (withFlameEffect && gameObject.activeInHierarchy)
            {
                var effect = Instantiate<CardBurnEffect>(cardBurnFX) as CardBurnEffect;
                effect.StartAnim(this);
            }
            else
                Destroy(gameObject);

            return true;
        }


        public void Populate(string elementId, int quantity)
        {

            _element = Registry.Retrieve<ICompendium>().GetElementById(elementId);
            try
            {

            SetQuantity(quantity);

            name = "Card_" + elementId;
            if (_element == null)
                return;

            DisplayInfo();
            DisplayIcon();
            ShowGlow(false, false);
            ShowCardDecayTimer(false);
            SetCardDecay(0f);
            lifetimeRemaining = _element.Lifetime;

            }
            catch (Exception e)
            {

                Debug.Log("Couldn't create element with ID " + elementId + " - " + e.Message);
                Retire(false);
            }
        }


        private void DisplayInfo()
		{
			text.text = _element.Label;
			stackBadge.gameObject.SetActive(Quantity > 1);
			stackCountText.text = Quantity.ToString();
        }

        private void DisplayIcon()
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Id);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;
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
            notifier.ShowElementDetails(_element);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            //remove any suitability glows
            Registry.Retrieve<TabletopManager>().ShowDestinationsForStack(null);
            base.OnEndDrag(eventData);
        }

        public override void InteractWithTokenDroppedOn(IElementStack stackDroppedOn)
        {
            if (stackDroppedOn.Id == this.Id && stackDroppedOn.AllowMerge())
            {
                stackDroppedOn.SetQuantity(stackDroppedOn.Quantity + this.Quantity);
                DraggableToken.resetToStartPos = false;
                SoundManager.PlaySfx("CardPutOnStack");
                this.Retire(false);
            }
        }

        public void SplitAllButNCardsToNewStack(int n)
        {
            if (Quantity > n)
            {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);

                cardLeftBehind.Populate(Id, Quantity - n);
                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(1);
                cardLeftBehind.transform.position = transform.position;
                var gateway = container.GetElementStacksManager();

               gateway.AcceptStack(cardLeftBehind);
            }
   
        }

        public bool AllowMerge()
        {
            return container.AllowStackMerge && !Decays;
        }

        protected override void StartDrag(PointerEventData eventData)
        {
			// A bit hacky, but it works: DID NOT start dragging from badge? Split cards 
			if (eventData.hovered.Contains(stackBadge) == false) 
            	SplitAllButNCardsToNewStack(1);

            Registry.Retrieve<TabletopManager>().ShowDestinationsForStack(this);


            base.StartDrag(eventData);
        }
        
        // IGlowableView implementation

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            glowImage.SetColor(color);
        }

        public void ShowGlow(bool glowState, bool instant) {
            if (glowState)
                glowImage.Show(instant);
            else
                glowImage.Hide(instant);                     
        }


        public void Decay(float interval)
        {
            if (!Decays)
                return;
            lifetimeRemaining = lifetimeRemaining - interval;

            if (lifetimeRemaining < 0)
                Retire(true);

            if(lifetimeRemaining<_element.Lifetime/2)
            { 
                ShowCardDecayTimer(true);
                SetCardDecayTime(lifetimeRemaining);
            }

            SetCardDecay(1-lifetimeRemaining/_element.Lifetime);
           
        }
        // Card Decay Timer
        public void ShowCardDecayTimer(bool showTimer) {
            decayView.gameObject.SetActive(showTimer);
        }

        public void SetCardDecayTime(float timeRemaining) {
           
            decayCountText.text = timeRemaining.ToString("0.0") + "s";
        }

        public void SetCardDecay(float percentage) {
            percentage = Mathf.Clamp01(percentage);
            artwork.color = new Color(1f - percentage, 1f - percentage, 1f - percentage, 1.5f - percentage);
        }


        // NOTE: THIS IS ALL DEMO TEST CODE SO YOU CAN SEE THE VISUALS
        //float currentTime = -5f;

        //void Update() {
        //    float decayDuration = 20f;
        //    float timeToShowTimer = 10f;

        //    currentTime += Time.deltaTime;

        //    SetCardDecay(currentTime / (decayDuration - timeToShowTimer));

        //    if (currentTime >= decayDuration - timeToShowTimer) {
        //        ShowCardDecayTimer(true);
        //        SetCardDecayTime(Mathf.Lerp(timeToShowTimer, 0, Mathf.Abs((currentTime - decayDuration + timeToShowTimer) / timeToShowTimer)));
        //    }
        //    else { 
        //        ShowCardDecayTimer(false);
        //    }

        //    if (currentTime > decayDuration) {
        //        Retire(true);
        //    }
        //}


    }
}
