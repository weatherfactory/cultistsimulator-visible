using System.Collections;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI
{
    public class DeckDetailsWindow : AbstractDetailsWindow
    {
        private DeckSpec _deckSpec;
        private int _cardsInNextDraw;

        // Use this for initialization

        protected override void UpdateContentAfterNavigation(NavigationArgs args)
        {
           UpdateContent();
        }
        private void SetDeck(DeckSpec deckSpec, int cardsInNextDraw)
        {
            _deckSpec = deckSpec;
            _cardsInNextDraw = cardsInNextDraw;
            
              ShowImage(ResourcesManager.GetSpriteForCardBack(_deckSpec.Id));
            
            ShowText(deckSpec.Label, deckSpec.Description);
        }

        public void ShowDeckDetails(DeckSpec deckSpec,int cardsInNextDraw)
        {
            if(this._deckSpec==deckSpec && gameObject.activeSelf)
                return;
            
            _deckSpec = deckSpec;
            _cardsInNextDraw = cardsInNextDraw;
            Show();
        }

        protected override void UpdateContent()
        {
            if(_deckSpec!=null)
                SetDeck (_deckSpec,0);
            
        }



        protected override void ClearContent()
        {
            _deckSpec = null;

        }


    }
}