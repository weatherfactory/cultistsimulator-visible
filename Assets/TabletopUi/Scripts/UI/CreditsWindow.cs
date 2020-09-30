using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.UI
{
    public class CreditsWindow: MonoBehaviour,ITokenObserver
    {
        [SerializeField] public ElementStackToken ElementStackPrefab;
        [SerializeField] public ExhibitCards CardsExhibit;
        [SerializeField] public TextMeshProUGUI Names;


        public bool Initialised { get; private set; }

        public void OnEnable()
        {
            Debug.Log("starting");
            if (!Initialised)
                Initialise();
        }

        private void Initialise()
        {
            CardsExhibit.Initialise();

            //List<string> creditCardIds = new List<string> { "reason", "passion", "health" }; 

            List<Element> creditCards = Registry.Get<ICompendium>().GetEntitiesAsList<Element>()
                .Where(e => e.Id.StartsWith("credits.")).ToList();

            foreach (var cc in creditCards)
            {
                var card=CardsExhibit.ProvisionElementStack(cc.Id, 1, Source.Fresh());
                card.AddObserver(this);
               
            }

            var firstCard = creditCards[0];

            CardsExhibit.HighlightCardWithId(firstCard.Id);
            Names.text = firstCard.Description;

            Initialised = true;

        }

        public void OnStackClicked(ElementStackToken stack, PointerEventData pointerEventData, Element element)
        {
            
            CardsExhibit.HighlightCardWithId(stack.EntityId);
            Names.text = element.Description;

        }

        public void OnStackDropped(ElementStackToken stack, EventArgs eventData)
        {
        }

        public void OnStackPointerEntered(ElementStackToken stack, PointerEventData pointerEventData)
        {
        }

        public void OnStackPointerExited(ElementStackToken stack, PointerEventData pointerEventData)
        {
        }

        public void OnStackDoubleClicked(ElementStackToken elementStackToken, PointerEventData eventData, Element element)
        {

        }
    }
}
