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
            List<Element> creditCards = Registry.Get<Compendium>().GetEntitiesAsList<Element>()
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

        public void NotifyTokensChangedForSphere(TokenInteractionEventArgs args)
        {
        //
        }

        public void OnTokenInteraction(TokenInteractionEventArgs args)
        {
           if(args.Interaction==Interaction.OnClicked || args.Interaction == Interaction.OnDoubleClicked)
           {
               CardsExhibit.HighlightCardWithId(args.Token.Element.Id);
             Responsibilities.text = args.Token.Element.Label;
            Names.text = args.Element.Description;
           }

           if(args.Interaction==Interaction.OnPointerEntered)
               args.Token.Emphasise();

           if (args.Interaction == Interaction.OnPointerExited)
           {
               if (Responsibilities.text != args.Token.Element.Label) // don't remove the highlight if the card is currently selected
                   args.Token.Understate();
           }
        }

    

        
    }
}
