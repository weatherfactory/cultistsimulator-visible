using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CanvasScalableUI : CanvasScaler,ISettingSubscriber {

	private float scaleFactorFactor = 1f;
    
    private Canvas canvas;

    private const float MultiplierForSettingValue = 0.25f; //we get a raw value from the setting config; reduce it accordingly
    private const float MinUIScaleSize = 0.5f;

    protected override void OnEnable()
    {
        canvas = GetComponent<Canvas>();
        base.OnEnable();
    }

	protected override void Start()
    {
		if(Application.isPlaying) //this is necessary because CanvasScalableUI inherits from CanvasScaler, which is ExecuteAlways.
        {
            var uiScaleSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.SCREENCANVASSIZE);
            if (uiScaleSetting != null)
            {
                uiScaleSetting.AddSubscriber(this);
                UpdateValueFromSetting(uiScaleSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.SCREENCANVASSIZE,2);
        }
	}

    public void UpdateValueFromSetting(object newValue)
    {
        var scale = MultiplierForSettingValue * (newValue is float ? (float)newValue : 0); ;
        scale = Mathf.Max(scale, MinUIScaleSize);

        SetScaleFactorFactor(scale);
	}


    // Here we get the currentZoom between 0 (zoomed in) and 1 (zoomed out)
    // We use that to evaluate the curve to get another value between 0 and 1. This distorts the zoom so that zooming out is slower
    // Then we use that value to get a scale factor between our min and max zoomScales and put that in the canvas
    void SetScaleFactorFactor(float scale)
    {
        scaleFactorFactor = Mathf.Max(0.01f, scale);
        Handle();
    }

	// The log base doesn't have any influence on the results whatsoever, as long as the same base is used everywhere.
	private const float kLogBase = 2;

	protected override void HandleScaleWithScreenSize()
	{
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);

		// Multiple display support only when not the main display. For display 0 the reported
		// resolution is always the desktops resolution since its part of the display API,
		// so we use the standard none multiple display method. (case 741751)
		int displayIndex = canvas.targetDisplay;
		if (displayIndex > 0 && displayIndex < Display.displays.Length)
		{
			Display disp = Display.displays[displayIndex];
			screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
		}

		float scaleFactor = 0;
		switch (m_ScreenMatchMode)
		{
			case ScreenMatchMode.MatchWidthOrHeight:
				{
					// We take the log of the relative width and height before taking the average.
					// Then we transform it back in the original space.
					// the reason to transform in and out of logarithmic space is to have better behavior.
					// If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
					// In normal space the average would be (0.5 + 2) / 2 = 1.25
					// In logarithmic space the average is (-1 + 1) / 2 = 0
					float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
					float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
					float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
					scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
					break;
				}
			case ScreenMatchMode.Expand:
				{
					scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
					break;
				}
			case ScreenMatchMode.Shrink:
				{
					scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
					break;
				}
		}

		scaleFactor *= scaleFactorFactor;

		SetScaleFactor(scaleFactor);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}




}
