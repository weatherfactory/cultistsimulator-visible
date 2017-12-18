using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasZoomTest : UIBehaviour {

    [Tooltip("This is the max window size we zoom out to, ensures that both sides are visible")]
    public Vector2 maxWindowSize = new Vector2(2400f, 1800f);
    private float zoomScaleIn = 1f; // 1 == 100% = pixel perfect zoom
    private float zoomScaleOut;     // Needs to be smaller than zoomScaleIn to enable zooming

    public float zoomSpeed = 0.1f;
    public AnimationCurve zoomCurve; // evaluated to provide some easing across the whole zoom range

    // ranges from 0 to 1, with 0 zoomed in all the way, 1 zoomed out all the way
    // Change these to adjust starting zoom
    private float currentZoom = 0.5f; 
    private float targetZoom = 0.5f;
    private const float zoomTolerance = 0.00001f; // snap when this close to target

    private Canvas canvas;
    private UIParticleCanvasScaler particleScaler;

    protected override void Start() {
        Init();
    }

    protected override void OnRectTransformDimensionsChange() {
        Init();
    }

    private void Init() {
        // Get us the maximum zoom size so that the max pixel window size is visible.
        zoomScaleOut = Mathf.Min(Screen.width / maxWindowSize.x, Screen.height / maxWindowSize.y);
        
        // We can't zoom out at all because our maxZoom would be larger than our minZoom
        if (zoomScaleOut >= zoomScaleIn) {
            canvas.scaleFactor = 1f; // Set our scale to 100%
            enabled = false; // Turn off to disable update
            return;
        }

        enabled = true;
        canvas = GetComponent<Canvas>();
        particleScaler = GetComponent<UIParticleCanvasScaler>();
        SetScale(currentZoom);
    }

    void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && targetZoom > 0f) {
            targetZoom -= 0.1f;
            targetZoom = Mathf.Clamp01(targetZoom);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && targetZoom < 1f) {
            targetZoom += 0.1f;
            targetZoom = Mathf.Clamp01(targetZoom);
        }

        if (targetZoom != currentZoom) {
            if (Mathf.Approximately(targetZoom, currentZoom))
                currentZoom = targetZoom;
            else
                currentZoom += (targetZoom - currentZoom) * Time.deltaTime * zoomSpeed;

            SetScale(currentZoom);
        }
    }

    public void SetTargetZoom(float value) {
        targetZoom = Mathf.Clamp01(value);
    }

    // Here we get the currentZoom between 0 (zoomed in) and 1 (zoomed out)
    // We use that to evaluate the curve to get another value between 0 and 1. This distorts the zoom so that zooming out is slower
    // Then we use that value to get a scale factor between our min and max zoomScales and put that in the canvas
    void SetScale(float zoom) {
        canvas.scaleFactor = Mathf.Lerp(zoomScaleIn, zoomScaleOut, zoomCurve.Evaluate(zoom));
        particleScaler.SetParticleScales();
    }

}
