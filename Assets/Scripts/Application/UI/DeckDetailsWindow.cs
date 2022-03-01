using System.Collections;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.Entities;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI
{
    public class DeckDetailsWindow : AbstractDetailsWindow
    {
        private DeckEffect _deckEffect;
        [SerializeField] private TextMeshProUGUI _drawsCount;

        // Use this for initialization

        protected override void UpdateContentAfterNavigation(NavigationArgs args)
        {
           UpdateContent();
        }
        private void SetDeckEffect(DeckEffect deckEffect)
        {
            _deckEffect = deckEffect;
            
              ShowImage(ResourcesManager.GetSpriteForCardBack(_deckEffect.DeckSpec.Id));
              ShowText(_deckEffect.DeckSpec.Label, _deckEffect.DeckSpec.Description);
              _drawsCount.text = Watchman.Get<ILocStringProvider>().Get("UI_UPCOMINGDRAWS") + deckEffect.Draws;
        }

        public void ShowDeckDetails(DeckEffect deckEffect)
        {

            if(_deckEffect==deckEffect && gameObject.activeSelf)
                return;

            _deckEffect = deckEffect;
            Show();
        }

        protected override void UpdateContent()
        {
            if(_deckEffect != null)
                SetDeckEffect (_deckEffect);
            
        }



        protected override void ClearContent()
        {
            _deckEffect = null;

        }


    }
}