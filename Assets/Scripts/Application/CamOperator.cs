using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.Constants;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;



[RequireComponent(typeof(Camera))]
public class CamOperator : MonoBehaviour {

    private Camera attachedCamera;
    private float currentTruckInput;
    private float currentPedestalInput;
    private float currentZoomInput;
    private Vector3 smoothTargetPosition;
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector2 AdjustedCameraBoundsMin;
    private Vector2 AdjustedCameraBoundsMax;

    [SerializeField]
    private  float pan_step_distance;
    [SerializeField]
    private float zoom_step_distance;
    
    public const float ZOOM_Z_CLOSE=-30f;

    public const float ZOOM_Z_MID=-600f;
    
    public const float ZOOM_Z_FAR =-1200f;

    [SerializeField] private float cameraMoveDuration;
    private float currentCameraMoveDuration; //overridden by eg pooint-to instructions
    private float currentMoveDurationRemaining = 0f;

    [SerializeField] private RectTransform navigationLimits;


    public event Action OnCameraArrived;

    public void Awake()
    {
        var w = new Watchman();
        w.Register(this);
    }


    protected void Start()
    {
        attachedCamera = gameObject.GetComponent<Camera>();
        smoothTargetPosition = attachedCamera.transform.position;
        SetNavigationLimitsBasedOnCurrentCameraHeight();

    }

    private void SetNavigationLimitsBasedOnCurrentCameraHeight()
    {
        AdjustedCameraBoundsMin = getCameraPositionAboveTableAdjustedForRotation(navigationLimits.rect.min, attachedCamera.transform.position.z);
        AdjustedCameraBoundsMax = getCameraPositionAboveTableAdjustedForRotation(navigationLimits.rect.max, attachedCamera.transform.position.z);
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
            smoothTargetPosition.z = ZOOM_Z_CLOSE;

       else if (args.AbsoluteTargetZoomLevel == ZoomLevel.Mid)
            smoothTargetPosition.z = ZOOM_Z_MID;

        else if (args.AbsoluteTargetZoomLevel == ZoomLevel.Far)
            smoothTargetPosition.z = ZOOM_Z_FAR;
        else
          currentZoomInput = args.CurrentZoomInput * zoom_step_distance;



    }



    public void ApplySmoothInputVector(Vector3 smoothInput)
    {
        smoothTargetPosition += smoothInput;
    }

    public void ApplySmoothInputVector(Vector3 smoothInput,float movementDuration)
    {
        smoothTargetPosition += smoothInput;
        currentCameraMoveDuration = movementDuration;
    }

    public void Update()
    {

       if(Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) > 10)
            currentMoveDurationRemaining -= Time.deltaTime;

            if (currentTruckInput != 0)
            {
                smoothTargetPosition.x += currentTruckInput;
                currentCameraMoveDuration = cameraMoveDuration; //reset to standard duration if we're moving manually again
            }

            if (currentPedestalInput != 0)
            {
                smoothTargetPosition.y += currentPedestalInput;
                currentCameraMoveDuration = cameraMoveDuration; //reset to standard duration if we're moving manually again
            }
                

            if (currentZoomInput != 0)
            {
                smoothTargetPosition.z -= currentZoomInput;
                smoothTargetPosition.z = Mathf.Clamp(smoothTargetPosition.z, ZOOM_Z_FAR, ZOOM_Z_CLOSE);
                currentCameraMoveDuration = cameraMoveDuration; //reset to standard duration if we're moving manually again
            }

            if (Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) > 10)
            {
                smoothTargetPosition = ClampToNavigationRect(navigationLimits, smoothTargetPosition);

                attachedCamera.transform.position = Vector3.SmoothDamp(attachedCamera.transform.position, smoothTargetPosition,
                    ref cameraVelocity, currentCameraMoveDuration);

                if (Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) < 10)
                    cameraHasArrived();

                SetNavigationLimitsBasedOnCurrentCameraHeight(); //so this is called every time the camera smooth-moves, which works but is clunky.

            }
        




    }

    private void cameraHasArrived()
    {
        smoothTargetPosition = attachedCamera.transform.position;
        currentCameraMoveDuration = cameraMoveDuration; //If we're at the target position, always reset to standard movement speed
        currentMoveDurationRemaining = 0f;
        OnCameraArrived?.Invoke();
    }

    private Vector3 ClampToNavigationRect(RectTransform limitTo, Vector3 targetPosition)
    {
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, AdjustedCameraBoundsMin.x, AdjustedCameraBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, AdjustedCameraBoundsMin.y, AdjustedCameraBoundsMax.y);

        return targetPosition;

    }

    public void OnGUI()
    {

        GUI.Label(new Rect(10, 10, 300, 20), $"currentCameraMoveDuration: {currentCameraMoveDuration}");
        GUI.Label(new Rect(10, 30, 300, 20), $"currentMoveDurationRemaining: {currentMoveDurationRemaining}");
        GUI.Label(new Rect(10, 50, 300, 20), $"camera position: {attachedCamera.transform.position}");
        GUI.Label(new Rect(10, 70, 300, 20), $"smoothTarget: {smoothTargetPosition}");
        GUI.Label(new Rect(10, 90, 300, 20), $"cam velocity: {cameraVelocity}");

    }


    public void PointCameraAtTableLevelVector2(Vector2 targetPosition,float secondsTakenToGetThere)
    {
        smoothTargetPosition = getCameraPositionAboveTableAdjustedForRotation(targetPosition, attachedCamera.transform.position.z);
        currentCameraMoveDuration = secondsTakenToGetThere;
        currentMoveDurationRemaining = secondsTakenToGetThere;
    }

    public void PointAtTableLevelWithZoomFactor(Vector2 targetPosition, float zoomFactor, float secondsTakenToGetThere,Action onArrival)
    {
        OnCameraArrived += onArrival;
        float targetHeight = attachedCamera.transform.position.z * zoomFactor;
        smoothTargetPosition = getCameraPositionAboveTableAdjustedForRotation(targetPosition, targetHeight);
        smoothTargetPosition.z = targetHeight;
        currentCameraMoveDuration = secondsTakenToGetThere;
        currentMoveDurationRemaining = secondsTakenToGetThere;
    }



    public Vector3 getCameraPositionAboveTableAdjustedForRotation(Vector2 tablePosition,float zPosition)
    {
        float angle = attachedCamera.transform.rotation.x;
        float adjacentSide = attachedCamera.transform.position.z;
        float tanOfAngle = Mathf.Tan(angle);
        float oppositeSide = adjacentSide * tanOfAngle;


        return new Vector3(tablePosition.x, tablePosition.y - oppositeSide, zPosition);
    }



}
