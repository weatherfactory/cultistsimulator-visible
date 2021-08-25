using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


[RequireComponent(typeof(Camera))]
public class CamOperator : MonoBehaviour {

    private Camera attachedCamera;
    private float currentTruckInput;
    private float currentPedestalInput;
    private float currentZoomInput;
    private Vector3 currentSnapInput;
    private Vector3 snapTargetPosition;
    private Vector3 smoothTargetPosition;
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector2 AdjustedCameraBoundsMin;
    private Vector2 AdjustedCameraBoundsMax;

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

    [SerializeField] private float cameraMoveDuration;

    [SerializeField] private RectTransform navigationLimits;


    public void Awake()
    {
        var w = new Watchman();
        w.Register(this);
    }


    protected void Start()
    {
        attachedCamera = gameObject.GetComponent<Camera>();
        snapTargetPosition = attachedCamera.transform.position;
        smoothTargetPosition = attachedCamera.transform.position;
        SetNavigationLimitsBasedOnCurrentCameraHeight();

    }

    private void SetNavigationLimitsBasedOnCurrentCameraHeight()
    {
        AdjustedCameraBoundsMin = getCameraPositionAboveTableAdjustedForRotation(navigationLimits.rect.min);
        AdjustedCameraBoundsMax = getCameraPositionAboveTableAdjustedForRotation(navigationLimits.rect.max);
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
            snapTargetPosition.z = zoom_z_close;

        if (args.AbsoluteTargetZoomLevel == ZoomLevel.Mid)
            snapTargetPosition.z = zoom_z_mid;

        if (args.AbsoluteTargetZoomLevel == ZoomLevel.Far)
            snapTargetPosition.z = zoom_z_far;

        //NB: this result is received when the zoom-increment key is lifted, at which point it'll be set to 0 and stop the zoom continuing
        //if we receive a zoom with an absolute value, that will also reset-and-halt any ongoing zoom effects

        currentZoomInput = args.CurrentZoomInput * zoom_step_distance;
        Debug.Log($"Zoom event {currentZoomInput}");


    }

    public void ApplySnapInputVector(Vector3 snapInput)
    {
        currentSnapInput += snapInput;
    }


    public void ApplySmoothInputVector(Vector3 smoothInput)
    {
        smoothTargetPosition += smoothInput;
    }

    public void Update()
    {

        if (currentSnapInput != Vector3.zero)
        {
           snapTargetPosition += currentSnapInput;
           currentSnapInput = Vector3.zero;

        }
        else
        {
            if (currentTruckInput != 0)
                smoothTargetPosition.x += currentTruckInput;
            if (currentPedestalInput != 0)
                smoothTargetPosition.y += currentPedestalInput;

            if (currentZoomInput != 0)
            {
                smoothTargetPosition.z -= currentZoomInput;
                smoothTargetPosition.z = Mathf.Clamp(smoothTargetPosition.z, zoom_z_far, zoom_z_close);
            }
        }

        if (Vector3. Distance(attachedCamera.transform.position,snapTargetPosition)>0)
        {
            snapTargetPosition = ClampToNavigationRect(navigationLimits, snapTargetPosition);
            attachedCamera.transform.position = snapTargetPosition;
            smoothTargetPosition = attachedCamera.transform.position;
        }

        else if (Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) > 0)
        {
            smoothTargetPosition = ClampToNavigationRect(navigationLimits, smoothTargetPosition);
            Debug.Log($"CamOps: SmoothTargetPosition after clamp{smoothTargetPosition}");
            
            attachedCamera.transform.position = Vector3.SmoothDamp(attachedCamera.transform.position, smoothTargetPosition,
                ref cameraVelocity, cameraMoveDuration);
            
            snapTargetPosition = attachedCamera.transform.position;

            SetNavigationLimitsBasedOnCurrentCameraHeight(); //so this is called every time the camera smooth-moves, which works but is clunky.

        }

    }

    private Vector3 ClampToNavigationRect(RectTransform limitTo, Vector3 targetPosition)
    {
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, AdjustedCameraBoundsMin.x, AdjustedCameraBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, AdjustedCameraBoundsMin.y, AdjustedCameraBoundsMax.y);

        return targetPosition;

    }

    public void OnGUI()
    {
        
        GUI.Label(new Rect(10,10,200,20),$"adjustedMin: {AdjustedCameraBoundsMin}");
        GUI.Label(new Rect(10, 30, 200, 20), $"adjustedMax: {AdjustedCameraBoundsMax}");
        GUI.Label(new Rect(10, 50, 200, 20), $"smoothTarget: {smoothTargetPosition}");

    }


    public void PointCameraAtTableLevelVector2(Vector2 targetPosition)
    {

        smoothTargetPosition = getCameraPositionAboveTableAdjustedForRotation(targetPosition);
    }

    private Vector3 getCameraPositionAboveTableAdjustedForRotation(Vector2 tablePosition)
    {
        float angle = attachedCamera.transform.rotation.x;
        float adjacentSide = attachedCamera.transform.position.z;
        float tanOfAngle = Mathf.Tan(angle);
        float oppositeSide = adjacentSide * tanOfAngle;


        return new Vector3(tablePosition.x, tablePosition.y - oppositeSide, attachedCamera.transform.position.z);
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
