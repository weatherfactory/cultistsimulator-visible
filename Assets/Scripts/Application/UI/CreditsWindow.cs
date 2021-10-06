using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using SecretHistories.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.UI
{
    public class CreditsWindow: MonoBehaviour,ISphereEventSubscriber
    {
        [SerializeField] public ExhibitCardsSphere CardsSphereExhibit;
        [SerializeField] public TextMeshProUGUI Responsibilities;
        [SerializeField] public TextMeshProUGUI Names;


        public void OnEnable()
        {
            List<Element> creditCards = Watchman.Get<Compendium>().GetEntitiesAsList<Element>()
                .Where(e => e.Id.StartsWith("credits.")).ToList();


            foreach (var cc in creditCards)
            {
                var t=new TokenCreationCommand().WithElementStack(cc.Id,1);
                t.Execute(new Context(Context.ActionSource.UI), CardsSphereExhibit);
            }

            var firstCard = creditCards[0];

            CardsSphereExhibit.HighlightCardWithId(firstCard.Id);
            Responsibilities.text = firstCard.Label;
            Names.text = firstCard.Description;


            CardsSphereExhibit.Subscribe(this);
        }

        public void OnDisable()
        {
         CardsSphereExhibit.RetireAllTokens();
         CardsSphereExhibit.Unsubscribe(this);
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
        //
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
           if(args.Interaction==Interaction.OnClicked || args.Interaction == Interaction.OnDoubleClicked)
           {
               CardsSphereExhibit.HighlightCardWithId(args.Token.Payload.Id);
             Responsibilities.text = args.Token.Payload.Label;
            Names.text = args.Payload.Description;
           }

           if(args.Interaction==Interaction.OnPointerEntered)
               args.Token.Emphasise();

           if (args.Interaction == Interaction.OnPointerExited)
           {
               if (Responsibilities.text != args.Token.Payload.Label) // don't remove the highlight if the card is currently selected
                   args.Token.Understate();
           }
        }

    

        
    }
}
