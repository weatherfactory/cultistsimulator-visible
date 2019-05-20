#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using Noon;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {
    public class TabletopImageBurner : MonoBehaviour {

        public enum ImageLayoutConfig { CenterOnToken, LowerLeftCorner }

        [SerializeField] Image burnImagePrefab;
        [SerializeField] AnimationCurve burnAlphaCurve;

        bool coroutineRunning = false;
        string burnImagePath = "burnImages/";

        List<Image> imagePool = new List<Image>();
        List<BurnImage> activeImages = new List<BurnImage>();
        
        public void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, ImageLayoutConfig config) { 
            var sprite = LoadBurnSprite(spriteName);

            if (sprite == null) {
                NoonUtility.Log("Can't find a sprite at " + burnImagePath + spriteName + "!",1);
                return;
            }

			SoundManager.PlaySfx("FXBurnImage");

            var image = GetUnusedImage();
            image.sprite = sprite;
            image.SetNativeSize();
            image.gameObject.SetActive(true);
            image.rectTransform.pivot = GetPivotFromConfig(config);
            image.transform.position = token.transform.position;
            image.transform.localRotation = Quaternion.identity;
            image.transform.localScale = Vector3.one * scale;

            activeImages.Add(new BurnImage(image, duration));

            if (!coroutineRunning)
                StartCoroutine(DecayImages());
        }

        IEnumerator DecayImages() {
            coroutineRunning = true;

            while (coroutineRunning) {
                for (int i = activeImages.Count - 1; i >= 0; i--) {
                    activeImages[i].timeSpent += Time.deltaTime;

                    if (activeImages[i].timeSpent > activeImages[i].duration) {
                        activeImages[i].image.gameObject.SetActive(false);
                        activeImages.RemoveAt(i);
                    }
                    else {
                        activeImages[i].image.canvasRenderer.SetAlpha(GetBurnAlpha(activeImages[i]));
                    }
                }

                yield return null;
                coroutineRunning = activeImages.Count > 0;
            }
        }

        float GetBurnAlpha(BurnImage burnImage) {
            return burnAlphaCurve.Evaluate(burnImage.timeSpent / burnImage.duration);
        }

        // Utility stuff

        Image GetUnusedImage() {
            for (int i = 0; i < imagePool.Count; i++) 
                if (imagePool[i].gameObject.activeSelf == false)
                    return imagePool[i];

            return AddImage();
        }

        Vector2 GetPivotFromConfig(ImageLayoutConfig config) {
            switch (config) {
                case ImageLayoutConfig.CenterOnToken:
                    return new Vector2(0.5f, 0.5f);
                case ImageLayoutConfig.LowerLeftCorner:
                default:
                    return Vector2.zero;
            }
        }

        Image AddImage() {
            var newImg = Instantiate<Image>(burnImagePrefab, transform) as Image;
            imagePool.Add(newImg);

            return newImg;
        }

        Sprite LoadBurnSprite(string imageName) {
            return ResourcesManager.GetSprite(burnImagePath, imageName, false);
        }

        class BurnImage {
            public Image image;
            public float duration;
            public float timeSpent;

            public BurnImage(Image image, float duration) {
                this.image = image;
                this.duration = duration;
                this.timeSpent = 0f;
            }
        }


        }
    }
