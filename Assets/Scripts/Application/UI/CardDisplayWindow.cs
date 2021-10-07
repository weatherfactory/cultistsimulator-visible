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
    public class CardDisplayWindow: MonoBehaviour,ISphereEventSubscriber
    {
        [SerializeField] public ExhibitCardsSphere CardsSphereExhibit;
        [SerializeField] public TextMeshProUGUI Label;
        [SerializeField] public TextMeshProUGUI Description;
        [SerializeField] public string IncludeCardsWhereIdStartsWith;


        public void OnEnable()
        {
            if (string.IsNullOrEmpty(IncludeCardsWhereIdStartsWith))
            {
                NoonUtility.LogWarning($"CardsDisplay IncludeCardsWhereIdStartsWith filter is empty. This would mean we'd try to display all the cards in the game, so we're not going to do that.");
                return;
            }

            List<Element> creditCards = Watchman.Get<Compendium>().GetEntitiesAsList<Element>()
                .Where(e => e.Id.StartsWith(IncludeCardsWhereIdStartsWith)).ToList();


            foreach (var cc in creditCards)
            {
                var t=new TokenCreationCommand().WithElementStack(cc.Id,1);
                t.Execute(new Context(Context.ActionSource.UI), CardsSphereExhibit);
            }

            var firstCard = CardsSphereExhibit.GetTokens().FirstOrDefault();

            if (firstCard != null)
                EmphasiseThisCardAndUnderstateOthers(firstCard);

            Label.text = firstCard.Payload.Label;
            Description.text = firstCard.Payload.Description;


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

               EmphasiseThisCardAndUnderstateOthers(args.Token);

               Label.text = args.Token.Payload.Label;
            Description.text = args.Payload.Description;
           }


        }

        private void EmphasiseThisCardAndUnderstateOthers(Token tokenToEmphasise)
        {
            var cardTokens = CardsSphereExhibit.GetElementTokens();

            foreach (var card in cardTokens)
            {
                if (card != tokenToEmphasise)
                    card.Understate();
                else
                    card.Emphasise();
            }
        }
    }
}
