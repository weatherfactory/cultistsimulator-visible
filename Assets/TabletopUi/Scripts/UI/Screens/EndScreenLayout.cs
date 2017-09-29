using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EndScreenLayout : UIBehaviour {

    public CanvasScaler canvasScaler;
    public RectTransform container;
    public RectTransform image;
    public RectTransform content;

    public float minTextWidth = 400f;
    public float maxTextWidth = 1000f;
    public float gutterWidth = 50f;
    public float marginWidth = 50f;

    DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

    protected override void OnEnable() {
        base.OnEnable();

        tracker.Add(this, content, DrivenTransformProperties.AnchoredPosition);
        tracker.Add(this, content, DrivenTransformProperties.SizeDeltaX);
        tracker.Add(this, image, DrivenTransformProperties.AnchoredPosition);
    }

    protected override void OnDisable() {
        base.OnDisable();

        tracker.Clear();
    }

    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
        DoLayout();
    }

    void DoLayout() { 
        var width = Screen.width;
        float imageWidth = image.rect.width;
        float xPosImage;
        float xPosText;
        float textWidth;
        
        textWidth = (width - marginWidth - imageWidth - gutterWidth - marginWidth) / canvasScaler.scaleFactor;
        textWidth = Mathf.Clamp(textWidth, minTextWidth, maxTextWidth);

        xPosImage = (width / canvasScaler.scaleFactor) / 2f - (imageWidth + textWidth + gutterWidth) / 2f;
        xPosText = xPosImage + imageWidth + gutterWidth;

        image.anchoredPosition = new Vector2(xPosImage, 0f);
        content.anchoredPosition = new Vector2(xPosText, 0f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
    }

}
