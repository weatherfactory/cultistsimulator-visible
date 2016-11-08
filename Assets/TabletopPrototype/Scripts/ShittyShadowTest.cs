using UnityEngine;
using System.Collections;

public class ShittyShadowTest : MonoBehaviour {

	// This is not a good class.
	// Ideally the ElementCard would set it's shadow whenever it sets it's position

	[SerializeField] RectTransform shadow;
	[SerializeField] RectTransform card;

	// This only works if the card is a child of a main group directly, not if it's parented to a window.
	void Update () {
		float zOffsetCard = card.anchoredPosition3D.z;

		shadow.anchoredPosition3D = new Vector3(-5f + zOffsetCard / 2f, -5f + zOffsetCard / 2f, -zOffsetCard);
	}
}
