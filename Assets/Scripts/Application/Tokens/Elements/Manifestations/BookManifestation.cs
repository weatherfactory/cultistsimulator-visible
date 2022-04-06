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
        private GameObject spine;

        [SerializeField]
        private Image spineImage;        [SerializeField]
        private GameObject flat;

        [SerializeField] private GameObject frontCover;
        [SerializeField]
        private Image frontCoverImage;

        [SerializeField] private GameObject backCover;
        [SerializeField]
        private Image backCoverImage; //possibly won't be used
        

        [SerializeField] private TextMeshProUGUI spineTitle;
        [SerializeField] private TextMeshProUGUI coverTitle;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GraphicFader _spineGlow;

        private const string BACK_COVER_SUFFIX = "_b";
        private const string SPINE_SUFFIX = "_";

        public override void Retire(RetirementVFX retirementVfx, Action callback)
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

        public void Initialise(IManifestable manifestable)
        {
            Sprite f = ResourcesManager.GetSpriteForFrontCover(manifestable.Icon);
            frontCoverImage.sprite = f;
            backCoverImage.sprite = f;
            Sprite s= ResourcesManager.GetSpriteForSpine(manifestable.Icon);
            spineImage.sprite=s;

            
            name = "book_" + manifestable.Id;
            spineTitle.text = manifestable.Label;
            coverTitle.text = manifestable.Label;
            
          Understate(); //show the backgroundy, on-shelf version by default
            UpdateVisuals(manifestable);
            
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
         
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
          
        }

        public void Shroud(bool instant)
        {

        }

        public void Emphasise()
        {
            if(!frontCover.activeSelf || spine.activeSelf)
            {
                frontCover.SetActive(true);
                spine.SetActive(false);
                UpdateRectTransformSize(frontCover);

            }
        }

        public void Understate()
        {
            if (frontCover.activeSelf || !spine.activeSelf)
            {
             frontCover.SetActive(false);
            spine.SetActive(true);
            UpdateObjToMatchImageDimensions(spine, spineImage);
            UpdateRectTransformSize(spine);
            }
        }

        private void UpdateObjToMatchImageDimensions(GameObject obj, Image image)
        {
            if (image == null || image.sprite == null)
                return;

            var height=image.sprite.texture.height;
            var width=image.sprite.texture.width;
            var objRT = obj.GetComponent<RectTransform>();
            objRT.sizeDelta = new Vector2(width, height);
        }

        private void UpdateRectTransformSize(GameObject toMatchObject)
        {


            RectTransform newSize = toMatchObject.GetComponent<RectTransform>();
            if (newSize == null)
            {
                NoonUtility.LogWarning($"Trying to update recttransform for bookmanifestation from {toMatchObject.name} but it doesn't have a recttransform");
                return;
            }

            RectTransform.sizeDelta = new Vector2(newSize.sizeDelta.x,
                newSize.sizeDelta.y);
            RectTransform.anchorMin = newSize.anchorMax;
            RectTransform.anchorMax = newSize.anchorMax;
        }

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit => true;


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
