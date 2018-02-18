using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SubtlePivotShift : MonoBehaviour {

	public bool moveHorizontal = true;
	public bool moveVertical = false;

    public Vector2 minPivot = Vector2.zero;
    public Vector2 maxPivot = Vector2.one;

	[Tooltip("If the pivot would move less than this, it doesn't move at all.")]
	public float minThreshold = 0.0001f;
	[Tooltip("How quickly the pivot blends towards the target.")]
	public float targetBlendAmount = 0.01f;
	[Tooltip("The maximum distance the pivot moves per frame.")]
	public float maxSpeed = 0.01f;

	[Tooltip("Padding which creates a smaller rect to get the target Pos from the mouse")]
	public Vector2 mouseScreenCornerPadding = new Vector2(50f, 50f);

    [Tooltip("How strong the mouse movement influences speed.")]
    public float mouseMultiplier = 20f;

	RectTransform rectTransform;
	Vector2 targetPivot = new Vector2();
	Vector2 posDelta;

	void Start() {
		rectTransform = GetComponent<RectTransform>();
		targetPivot = rectTransform.pivot;
	}

	void Update() {
		if (!moveHorizontal && !moveVertical)
			return;

		UpdateForMousePos();
	}

	// -- MOUSE ----------------------------------------------------

	void UpdateForMousePos() {
		SetTargetFromMouse();
		ClampTargetPos();

		posDelta = (targetPivot - rectTransform.pivot) * mouseMultiplier * targetBlendAmount * Time.deltaTime;

		if (posDelta.magnitude > maxSpeed)
			posDelta = posDelta.normalized * maxSpeed;
		else if (posDelta.magnitude < minThreshold)
			return;

		rectTransform.pivot += posDelta;
		ClampRectPivot();
	}

	void SetTargetFromMouse() {
		if (moveHorizontal) { 
			targetPivot.x = (Input.mousePosition.x - mouseScreenCornerPadding.x) / (Screen.width - mouseScreenCornerPadding.x * 2f);
            targetPivot.x = minPivot.x + (targetPivot.x * (maxPivot.x - minPivot.x));
        }

        if (moveVertical) {
            targetPivot.y = (Input.mousePosition.y - mouseScreenCornerPadding.y) / (Screen.height - mouseScreenCornerPadding.y * 2f);
            targetPivot.y = minPivot.y + (targetPivot.y * (maxPivot.y - minPivot.y));
        }
    }

	void ClampTargetPos() {
		targetPivot = new Vector2(Mathf.Clamp01(targetPivot.x), Mathf.Clamp01(targetPivot.y));
	}

	void ClampRectPivot() {
		rectTransform.pivot = new Vector2(Mathf.Clamp01(rectTransform.pivot.x), Mathf.Clamp01(rectTransform.pivot.y));
	}


}
