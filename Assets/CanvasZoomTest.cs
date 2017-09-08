using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasZoomTest : MonoBehaviour {

    [Tooltip("This is the max window size we zoom out to, ensures that both sides are visible")]
    public Vector2 maxWindowSize = new Vector2(2400f, 1800f);
    private float zoomScaleIn = 1f; // 1 == pixel perfect zoom
    private float zoomScaleOut;     // Needs to be smalle than zoomScaleIn

    public float zoomSpeed = 0.1f;
    public AnimationCurve zoomCurve; // evaluated to provide some easing across the whole zoom range

    // ranges from 0 to 1, with 0 zoomed in all the way, 1 zoomed out all the way
    private float currentZoom = 0.5f; 
    private float targetZoom = 0.5f;
    private const float zoomTolerance = 0.00001f; // snap when this close to target

    private Canvas canvas;

    void Start () {
        // Get us the maximum zoom size so that the max pixel window size is visible.
        zoomScaleOut = Mathf.Min(Screen.width / maxWindowSize.x, Screen.height / maxWindowSize.y); 

        // We can't zoom out at all because our maxZoom would be larger than our minZoom
        if (zoomScaleOut >= zoomScaleIn) {
            enabled = false; // Turn off to disable update
            return;
        }

        canvas = GetComponent<Canvas>();
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

    void SetScale(float zoom) {
        canvas.scaleFactor = Mathf.Lerp(zoomScaleIn, zoomScaleOut, zoomCurve.Evaluate(zoom));
    }

}
