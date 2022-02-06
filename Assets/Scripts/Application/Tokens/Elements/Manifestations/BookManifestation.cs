using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class BookManifestation : BasicManifestation, IManifestation, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image frontCover;
        [SerializeField]
        private Image backCover;
        [SerializeField]
        private Image spine;

        [SerializeField] private TextMeshProUGUI spineTitle;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GraphicFader _glow;

        private const string BACK_COVER_SUFFIX = "_b";
        private const string SPINE_SUFFIX = "_";

        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            Destroy(gameObject);
            callback();
        }

        public bool CanAnimateIcon()
        {
            return false; //if we add animations, the frames stuff should go in a distinct class
        }

        public void BeginIconAnimation()
        {
            //if we add animations, the frames stuff should go in a distinct class
        }

        private string GetBackCoverImageName(string frontCoverImageName)
        {
            return $"{frontCoverImageName}{BACK_COVER_SUFFIX}";
        }

        private string GetSpineImageName(string frontCoverImageName)
        {
            return $"{frontCoverImageName}{SPINE_SUFFIX}";
        }

        public void Initialise(IManifestable manifestable)
        {
            Sprite f = ResourcesManager.GetSpriteForElement(manifestable.Icon);
            frontCover.sprite = f;
            Sprite s= ResourcesManager.GetSpriteForElement(GetSpineImageName(manifestable.Icon));
            spine.sprite=s;
            Sprite b = ResourcesManager.GetSpriteForElement(GetBackCoverImageName(manifestable.Icon));
            backCover.sprite = b;
            
            name = "book_" + manifestable.Id;
            spineTitle.text = manifestable.Label;
            
            UpdateVisuals(manifestable);
            
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
         //
        }


        public void OnBeginDragVisuals()
        {
         //
        }

        public void OnEndDragVisuals()
        {
           //
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
           //
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
           //
        }

        public bool NoPush => false;
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
        public bool RequestingNoSplit => true;
        public void DoMove(RectTransform tokenRectTransform)
        {
           //
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(BookGhost), this.RectTransform);
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
