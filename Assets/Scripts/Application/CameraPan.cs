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
public class CameraPan : MonoBehaviour {

    private Camera attachedCamera;
    private float currentTruckInput;
    private float currentPedestalInput;
    [SerializeField]
    private float panDuration;
    [SerializeField]
    private  float pan_step_distance;
    
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

    public void Update()
    {
        if (currentTruckInput != 0)
            cameraTargetPosition.x += currentTruckInput;
        if (currentPedestalInput != 0)
            cameraTargetPosition.y += currentPedestalInput;

        if (attachedCamera.transform.position != cameraTargetPosition)
        {
            attachedCamera.transform.position = Vector3.SmoothDamp(attachedCamera.transform.position, cameraTargetPosition,
                ref cameraVelocity, panDuration);
        }
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
