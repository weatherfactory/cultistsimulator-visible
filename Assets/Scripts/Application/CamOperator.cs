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
public class CamOperator : MonoBehaviour {

    private Camera attachedCamera;
    private float currentTruckInput;
    private float currentPedestalInput;
    private float currentZoomInput;
    [SerializeField]
    private float cameraMovementDuration;
    [SerializeField]
    private  float pan_step_distance;
    [SerializeField]
    private float zoom_step_distance;
    [SerializeField]
    private float zoom_z_close;
    [SerializeField]
    private float zoom_z_mid;
    [SerializeField]
    private float zoom_z_far;


    private Vector3 cameraTargetPosition;
    private Vector3 cameraVelocity=Vector3.zero;

    public void Awake()
    {
        var w = new Watchman();
        w.Register(this);
    }


    protected void Start()
    {
        attachedCamera = gameObject.GetComponent<Camera>();
        cameraTargetPosition = attachedCamera.transform.position;
    }

    public void OnTruckEvent(TruckEventArgs args)
    {
        currentTruckInput = args.CurrentTruckInput* pan_step_distance;
        Debug.Log($"Truck event {currentTruckInput}");
    }

    public void OnPedestalEvent(PedestalEventArgs args)
    {
        currentPedestalInput = args.CurrentPedestalInput * pan_step_distance;
        Debug.Log($"Pedestal event {currentPedestalInput}");
    }

    public void OnZoomEvent(ZoomLevelEventArgs args)
    {


        if (args.AbsoluteTargetZoomLevel == ZoomLevel.Close)
            cameraTargetPosition.z = zoom_z_close;

        if (args.AbsoluteTargetZoomLevel == ZoomLevel.Mid)
            cameraTargetPosition.z = zoom_z_mid;

        if (args.AbsoluteTargetZoomLevel == ZoomLevel.Far)
            cameraTargetPosition.z = zoom_z_far;

        //NB: this result is received when the zoom-increment key is lifted, at which point it'll be set to 0 and stop the zoom continuing
        //if we receive a zoom with an absolute value, that will also reset-and-halt any ongoing zoom effects

        currentZoomInput = args.CurrentZoomInput * zoom_step_distance;
        Debug.Log($"Zoom event {currentZoomInput}");

    }

    public void Update()
    {
        if (currentTruckInput != 0)
            cameraTargetPosition.x += currentTruckInput;
        if (currentPedestalInput != 0)
            cameraTargetPosition.y += currentPedestalInput;

        if (currentZoomInput != 0)
            cameraTargetPosition.z -= currentZoomInput;

        if (attachedCamera.transform.position != cameraTargetPosition)
        {
            attachedCamera.transform.position = Vector3.SmoothDamp(attachedCamera.transform.position, cameraTargetPosition,
                ref cameraVelocity, cameraMovementDuration);
        }
    }

    public void PointCameraAtTableLevelVector2(Vector2 targetPosition)
    {
        //we avoid changing the camera z as long as field of view is our zoom solution
        float angle = attachedCamera.transform.rotation.x;
        float adjacentSide = attachedCamera.transform.position.z;
        float tanOfAngle = Mathf.Tan(angle);
        float oppositeSide = adjacentSide * tanOfAngle;

        cameraTargetPosition = new Vector3(targetPosition.x, targetPosition.y-oppositeSide, attachedCamera.transform.position.z);
    }

    public IEnumerator FocusOn(Vector3 targetPos, float zoomDuration)
    {

        float time = 0f;
        Vector3 startPos = attachedCamera.transform.position;
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y - 60, startPos.z); //this y is a fudge that should actually be calculated on the fly


        while (time < zoomDuration)
        {
            attachedCamera.transform.position = Vector3.Lerp(startPos, endPos, time / zoomDuration);
            yield return null;
            time += Time.deltaTime;
        }


    }
}
