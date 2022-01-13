using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class BookManifestation : MonoBehaviour, IManifestation, IPointerEnterHandler, IPointerExitHandler
    {
        private Image frontCover;
        private Image backCover;
        private Image spine;
        private string _entityId;

        private const string BACK_COVER_SUFFIX = "_b";
        private const string SPINE_SUFFIX = "_";

        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
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
            
            _entityId = manifestable.EntityId;
            UpdateVisuals(manifestable);
            
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            UpdateVisuals(manifestable);
        }

        public void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
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
            return NullGhost.Create(this);
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
