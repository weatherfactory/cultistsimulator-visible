using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TODO: THIS IS PROBABLY NOW REDUNDANT

namespace Assets.CS.TabletopUI
{
    public class StoredElementStack : IElementStack
    {

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GraphicFader glowImage;
        [SerializeField] GameObject stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [SerializeField] CardEffectRemoveColorAnim cardBurnFX;

        private Element _element;
        private int _quantity;
        private ITokenTransformWrapper currentWrapper;
        private float lifetimeRemaining;

        public string Id
        {
            get { return _element == null ? null : _element.Id; }
        }

        public string SaveLocationInfo { get; set; }

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
 
            }
 
        }


        public Dictionary<string, string> GetXTriggers()
        {
            return _element.XTriggers;
        }

        public void ModifyQuantity(int change)
        {
            SetQuantity(_quantity + change);
        }

        public bool Retire()
        {
            return Retire(true);
        }

        public bool Retire(bool withVFX)
        {
            if (Defunct)
                return false;
            
            Defunct = true;
            return true;
        }


        public void Populate(string elementId, int quantity)
        {

            _element = Registry.Retrieve<ICompendium>().GetElementById(elementId);
            try
            {
                if (_element == null)
                {
                    NoonUtility.Log("Couldn't find element with ID " + elementId + " - ");
                    Retire(false);
                }

                SetQuantity(quantity);
                
              
                lifetimeRemaining = _element.Lifetime;

            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't create element with ID " + elementId + " - " + e.Message);
                Retire(false);
            }
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

   


        public void SplitAllButNCardsToNewStack(int n)
        {
    throw new NotImplementedException();

        }

        /// <summary>
        /// always allow merges for stored stacks
        /// </summary>
        /// <returns></returns>
        public bool AllowMerge()
        {
            return true && !Decays;
        }


        public void Decay(float interval)
        {
            if (!Decays)
                return;
            lifetimeRemaining = lifetimeRemaining - interval;

            if (lifetimeRemaining < 0)
                Retire(true);

        }
     

        

    }
}
