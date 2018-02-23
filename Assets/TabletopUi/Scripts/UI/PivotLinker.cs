using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PivotLinker : MonoBehaviour {

	[Tooltip("The RectTransform whos pivot will be mirrored on this object")]
	[SerializeField] RectTransform targetTransform;
	RectTransform rectTransform;

	void Awake () {
		if (targetTransform == null)
			Destroy(this);
		
		rectTransform = GetComponent<RectTransform>();
	}

	void Update() {
		rectTransform.pivot = targetTransform.pivot;
	}
}
