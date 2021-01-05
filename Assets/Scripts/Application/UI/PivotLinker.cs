using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PivotLinker : MonoBehaviour {

	[Tooltip("The RectTransform whos pivot will be mirrored on this object")]
#pragma warning disable 649
	[SerializeField] RectTransform targetTransform;
#pragma warning restore 649
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
