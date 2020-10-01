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
        [SerializeField] public ExhibitCards CardsExhibit;
        [SerializeField] public TextMeshProUGUI Responsibilities;
        [SerializeField] public TextMeshProUGUI Names;


        public void OnEnable()
        {
            List<Element> creditCards = Registry.Get<ICompendium>().GetEntitiesAsList<Element>()
                .Where(e => e.Id.StartsWith("credits.")).ToList();

            foreach (var cc in creditCards)
            {
                var card=CardsExhibit.ProvisionElementStack(cc.Id, 1, Source.Fresh(),new Context(Context.ActionSource.UI));
                card.AddObserver(this);

            }

            var firstCard = creditCards[0];

            CardsExhibit.HighlightCardWithId(firstCard.Id);
            Responsibilities.text = firstCard.Label;
            Names.text = firstCard.Description;
        }

        public void OnDisable()
        {
         CardsExhibit.GetElementStacksManager().RemoveAllStacks();

        }



        public void OnStackClicked(ElementStackToken stack, PointerEventData pointerEventData, Element element)
        {
            
            CardsExhibit.HighlightCardWithId(stack.EntityId);
            Responsibilities.text = element.Label;
            Names.text = element.Description;

        }

        public void OnStackDropped(ElementStackToken stack, PointerEventData eventData)
        {
        }

        public void OnStackPointerEntered(ElementStackToken stack, PointerEventData pointerEventData)
        {
            stack.Emphasise();
        }

        public void OnStackPointerExited(ElementStackToken stack, PointerEventData pointerEventData)
        {
            if(Responsibilities.text!=stack.Label) // don't remove the highlight if the card is currently selected
             stack.Understate();
        }

        public void OnStackDoubleClicked(ElementStackToken elementStackToken, PointerEventData eventData, Element element)
        {

        }
    }
}
