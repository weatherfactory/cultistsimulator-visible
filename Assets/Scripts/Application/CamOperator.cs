using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.Constants;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;



[RequireComponent(typeof(Camera))]
public class CamOperator : MonoBehaviour {

    private Camera attachedCamera;
    private float currentTruckInput;
    private float currentPedestalInput;
    private float currentZoomInput;
    private Vector3 initialPosition;
    private Vector3 smoothTargetPosition;
    private Vector2 AdjustedCameraBoundsMin;
    private Vector2 AdjustedCameraBoundsMax;

    [SerializeField]
    private  float pan_step_distance;
    [SerializeField]
    private float zoom_step_distance;
    
    public float ZOOM_Z_CLOSE=-500f;

    public float ZOOM_Z_MID=-900f;
    
    public  float ZOOM_Z_FAR =-1500f;

    [SerializeField] private float defaultCameraMoveDuration;
    private float moveDuration; //overridden by eg point-to instructions
    private float timeSpentMoving = 0f;

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
     //   Debug.Log($"Truck event {currentTruckInput}");
    }

    public void OnPedestalEvent(PedestalEventArgs args)
    {
        currentPedestalInput = args.CurrentPedestalInput * pan_step_distance;
     //   Debug.Log($"Pedestal event {currentPedestalInput}");
    }



    public void OnZoomEvent(ZoomLevelEventArgs args)
    {
        initialPosition = attachedCamera.transform.position;
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
        initialPosition = attachedCamera.transform.position;
        smoothTargetPosition += smoothInput;
    }

    public void ApplySmoothInputVector(Vector3 smoothInput,float movementDuration)
    {
        initialPosition = attachedCamera.transform.position;
        smoothTargetPosition += smoothInput;
        moveDuration = movementDuration;
    }

    public void Update()
    {
        if (TryMoveAtScreenEdge())
            return;

       if(Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) > 10)
            timeSpentMoving += Time.deltaTime;

       if (currentTruckInput != 0)
       {
           initialPosition = attachedCamera.transform.position;
            smoothTargetPosition.x += currentTruckInput;
           moveDuration = defaultCameraMoveDuration; //reset to standard duration if we're moving manually again
       }

       if (currentPedestalInput != 0)
       {
           initialPosition = attachedCamera.transform.position;
            smoothTargetPosition.y += currentPedestalInput;
           moveDuration = defaultCameraMoveDuration; //reset to standard duration if we're moving manually again
       }
                

       if (currentZoomInput != 0)
       {
           initialPosition = attachedCamera.transform.position;
            smoothTargetPosition.z -= currentZoomInput;
           smoothTargetPosition.z = Mathf.Clamp(smoothTargetPosition.z, ZOOM_Z_FAR, ZOOM_Z_CLOSE);
           moveDuration = defaultCameraMoveDuration; //reset to standard duration if we're moving manually again
       }

       if (Vector3.Distance(attachedCamera.transform.position, smoothTargetPosition) < 1)
           cameraHasArrived();
       else
       {
           smoothTargetPosition = ClampToNavigationRect(navigationLimits, smoothTargetPosition);

           attachedCamera.transform.position = Vector3.Lerp(initialPosition, smoothTargetPosition,
               timeSpentMoving / moveDuration);


           SetNavigationLimitsBasedOnCurrentCameraHeight(); //so this is called every time the camera smooth-moves, which works but is clunky.
       }


}

    private bool TryMoveAtScreenEdge()
    {
        const int EDGE_MARGIN_PX = 75;
        const float EDGE_SPEED = 180f;
        bool edgeMoved = false;
        var topEdge = new Rect(1f, Screen.height - EDGE_MARGIN_PX, Screen.width, EDGE_MARGIN_PX);
        var bottomEdge = new Rect(1f, 1f, Screen.width, EDGE_MARGIN_PX/3);  //bottom edge of the screen is a nuisance; we want people to be able to click on status bar without scrolling
        var leftEdge = new Rect(1f, 1f, EDGE_MARGIN_PX, Screen.height);
        var rightEdge = new Rect(Screen.width - EDGE_MARGIN_PX, 1f, EDGE_MARGIN_PX, Screen.height);

        Vector3 edgeScrollMove=Vector3.zero;
        if (topEdge.Contains(Mouse.current.position.ReadValue()) && !Watchman.Get<DebugTools>().isActiveAndEnabled)
        {
            edgeScrollMove.y = EDGE_SPEED;
            edgeMoved = true;
        }
        else if (bottomEdge.Contains(Mouse.current.position.ReadValue()))
        {
            edgeScrollMove.y = -EDGE_SPEED;
            edgeMoved = true;
        }
        else if (leftEdge.Contains(Mouse.current.position.ReadValue()) && !Watchman.Get<DebugTools>().isActiveAndEnabled)
        {
            edgeScrollMove.x= -EDGE_SPEED;
            edgeMoved = true;
        }
        else if (rightEdge.Contains(Mouse.current.position.ReadValue()))
        {
            edgeScrollMove.x = EDGE_SPEED;
            edgeMoved = true;
        }

        if (!edgeMoved)
            return false; //need to branch here, otherwise the lerp move below means the camera keeps re-arriving in the main loop


        Vector3 targetPosition = attachedCamera.transform.position + edgeScrollMove;

        attachedCamera.transform.position = Vector3.Lerp(attachedCamera.transform.position, targetPosition,
            Time.deltaTime);
        cameraHasArrived();
        return true;
        
    }

    private void cameraHasArrived()
    {
        smoothTargetPosition = attachedCamera.transform.position;
        moveDuration = defaultCameraMoveDuration; //If we're at the target position, always reset to standard movement speed
        timeSpentMoving = 0f;
        OnCameraArrived?.Invoke();
    }

    private Vector3 ClampToNavigationRect(RectTransform limitTo, Vector3 targetPosition)
    {
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, AdjustedCameraBoundsMin.x, AdjustedCameraBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, AdjustedCameraBoundsMin.y, AdjustedCameraBoundsMax.y);

        return targetPosition;

    }

    //public void OnGUI()
    //{

    //    GUI.Label(new Rect(10, 10, 300, 20), $"moveDuration: {moveDuration}");
    //    GUI.Label(new Rect(10, 30, 300, 20), $"timeSpentMoving: {timeSpentMoving}");
    //    GUI.Label(new Rect(10, 50, 300, 20), $"camera position: {initialPosition}");
    //    GUI.Label(new Rect(10, 70, 300, 20), $"camera position: {attachedCamera.transform.position}");
    //    GUI.Label(new Rect(10, 90, 300, 20), $"smoothTarget: {smoothTargetPosition}");

    //}


    public void PointCameraAtTableLevelVector2(Vector2 targetPosition,float secondsTakenToGetThere)
    {
        smoothTargetPosition = getCameraPositionAboveTableAdjustedForRotation(targetPosition, attachedCamera.transform.position.z);
        moveDuration = secondsTakenToGetThere;
        timeSpentMoving = secondsTakenToGetThere;
    }

    public void PointAtTableLevelWithZoomFactor(Vector2 targetPosition, float zoomFactor, float secondsTakenToGetThere,Action onArrival)
    {
        float targetHeight = attachedCamera.transform.position.z * zoomFactor;
        PointAtTableLevelAtHeight(targetPosition,targetHeight,secondsTakenToGetThere,onArrival);
        
        }

    public void PointAtTableLevelAtHeight(Vector2 targetPosition, float targetHeight, float secondsTakenToGetThere, Action onArrival)
    {
        OnCameraArrived += onArrival;
        smoothTargetPosition = getCameraPositionAboveTableAdjustedForRotation(targetPosition, targetHeight);
        smoothTargetPosition.z = targetHeight;
        moveDuration = secondsTakenToGetThere;
        timeSpentMoving = secondsTakenToGetThere;
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
