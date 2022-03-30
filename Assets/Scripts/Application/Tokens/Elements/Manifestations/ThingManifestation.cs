using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens.Ghosts;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    public class ThingManifestation: BasicManifestation, IManifestation, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image artwork;
        [SerializeField] public TextMeshProUGUI label;


        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GraphicFader _spineGlow;

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
           //
        }

        public void Initialise(IManifestable manifestable)
        {
      

            UpdateVisuals(manifestable);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            Sprite i = ResourcesManager.GetSpriteForElement(manifestable.Icon);
            artwork.sprite = i;


            name = manifestable.Id;
            label.text = manifestable.Label;
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
           //
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
         //
        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
          //
        }

        public void Shroud(bool instant)
        {
          //
        }

        public void Emphasise()
        {
            //
        }

        public void Understate()
        {
           //
        }

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit { get; }
        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(ThingGhost), this.RectTransform);
            return newGhost;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           //
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //
        }
    }
}
