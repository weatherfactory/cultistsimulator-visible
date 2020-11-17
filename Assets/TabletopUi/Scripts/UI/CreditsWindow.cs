using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.UI
{
    public class CreditsWindow: MonoBehaviour,ISphereEventSubscriber
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
                var card=CardsExhibit.ProvisionElementStackToken(cc.Id, 1, Source.Fresh(),new Context(Context.ActionSource.UI));
            }

            var firstCard = creditCards[0];

            CardsExhibit.HighlightCardWithId(firstCard.Id);
            Responsibilities.text = firstCard.Label;
            Names.text = firstCard.Description;
        }

        public void OnDisable()
        {
         CardsExhibit.RetireAllTokens();

        }



        public void NotifyTokensChangedForSphere(TokenEventArgs args)
        {
        //
        }

        public void OnTokenClicked(TokenEventArgs args)
        {

            CardsExhibit.HighlightCardWithId(args.Token.Element.Id);
            Responsibilities.text = args.Token.Element.Label;
            Names.text = args.Element.Description;
        }

        public void OnTokenReceivedADrop(TokenEventArgs args)
        {
            //
        }

        public void OnTokenPointerEntered(TokenEventArgs args)
        {
            args.Token.Emphasise();
        }

        public void OnTokenPointerExited(TokenEventArgs args)
        {
            if (Responsibilities.text != args.Token.Element.Label) // don't remove the highlight if the card is currently selected
                args.Token.Understate();
        }

        public void OnTokenDoubleClicked(TokenEventArgs args)
        {
            OnTokenClicked(args);
        }

        public void OnTokenDragged(TokenEventArgs args)
        {
            
        }
    }
}
