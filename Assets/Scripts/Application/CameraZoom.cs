using System.Collections;
using System.Collections.Generic;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour {


    Camera zoomCam;

    public float zoomScaleIn = 2f; // 1 == 100% = pixel perfect zoom
    public float zoomScaleOut = 4f;     // Needs to be smaller than zoomScaleIn to enable zooming

    public float zoomSpeed = 0.1f;
    public AnimationCurve zoomCurve; // evaluated to provide some easing across the whole zoom range

    // ranges from 0 to 1, with 0 zoomed in all the way, 1 zoomed out all the way
    //SO NB LOWER VALUES ARE LESS DISTANCE, NOT LESS ZOOM
    // Change these to adjust starting zoom
    private float currentZoom = 0.6f; 
    private float targetZoom = 0.6f;
    private float currentZoomInput = 0f;
    public const float ZOOM_BASE_INCREMENT = 0.025f;
    private const float zoomTolerance = 0.00001f; // snap when this close to target

    public bool cameraZoomEnabled = true;
    



    public void Awake()
    {
        var w=new Watchman();
        w.Register(this);
    }


    protected void Start() {
        enabled = true;
        zoomCam = GetComponent<Camera>();
        SetFieldOfView(currentZoom);
    }

    public void OnZoomEvent(ZoomLevelEventArgs args)
    {
      
           currentZoomInput = args.CurrentZoomInput;
       
    }

    void Update ()
    {
        if(cameraZoomEnabled)
            UpdateZoomState();
    }

    private void UpdateZoomState()
    {
        if (currentZoomInput < 0 || currentZoomInput > 0)
            SetTargetZoom(targetZoom + currentZoomInput * ZOOM_BASE_INCREMENT);

        if (targetZoom != currentZoom)
        {
            if (Mathf.Approximately(targetZoom, currentZoom))
                currentZoom = targetZoom;
            else
                currentZoom += (targetZoom - currentZoom) * Time.deltaTime * zoomSpeed;

            SetFieldOfView(currentZoom);
        }
    }

    public void SetTargetZoom(float value) {
        targetZoom = Mathf.Clamp01(value);
    }

    public void StartFixedZoom(float value, float duration) {
        targetZoom = Mathf.Clamp01(value);

        var zoomDiff = Mathf.Abs(currentZoom - targetZoom);
        zoomSpeed = zoomDiff / duration;
    }


    // Here we get the currentZoom between 0 (zoomed in) and 1 (zoomed out)
    // We use that to evaluate the curve to get another value between 0 and 1. This distorts the zoom so that zooming out is slower
    // Then we use that value to get a scale factor between our min and max zoomScales and put that in the canvas
    void SetFieldOfView(float zoom) {
        zoomCam.fieldOfView = Mathf.Lerp(zoomScaleIn, zoomScaleOut, zoomCurve.Evaluate(zoom));
    }


}
