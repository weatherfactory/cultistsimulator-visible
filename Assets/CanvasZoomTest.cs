using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasZoomTest : MonoBehaviour {

    private Canvas canvas;
    private float currentZoom = 0.5f;
    private float targetZoom = 0.5f;

    public float zoomSpeed = 0.1f;
    public AnimationCurve zoomCurve;

	void Start () {
        canvas = GetComponent<Canvas>();
        SetScale(currentZoom);
	}
	
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && targetZoom < 1f) {
            targetZoom += 0.1f;
            targetZoom = Mathf.Clamp01(targetZoom);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && targetZoom > 0) {
            targetZoom -= 0.1f;
            targetZoom = Mathf.Clamp01(targetZoom);
        }

        if (targetZoom != currentZoom) {
            currentZoom += (targetZoom - currentZoom) * Time.deltaTime * zoomSpeed;
            SetScale(currentZoom);
        }
    }

    void SetScale(float zoom) {
        canvas.scaleFactor = zoomCurve.Evaluate(zoom);
    }

}
