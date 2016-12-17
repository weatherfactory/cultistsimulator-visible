using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    /// <summary>
    /// NOT CURRENTLY IN USE
    /// </summary>
	public class RecipeSlotIcon : MonoBehaviour {

		public Sprite iconLocked;
		public Sprite iconConsumes;
		public Sprite iconOngoing;
		public Sprite iconGreedy;

		[SerializeField] Graphic border;
		[SerializeField] Image sprite;

		public void Hide() {
			gameObject.SetActive(false);
		}

		public void SetSprite(RecipeSlot.SlotModifier modifier) {
			gameObject.SetActive(true);
			sprite.sprite = GetSprite(modifier);
		}

		Sprite GetSprite(RecipeSlot.SlotModifier modifier) {
			switch(modifier) {
			case RecipeSlot.SlotModifier.Consuming:
				return iconConsumes;
			case RecipeSlot.SlotModifier.Greedy:
				return iconGreedy;
			case RecipeSlot.SlotModifier.Locked:
				return iconLocked;
			case RecipeSlot.SlotModifier.Ongoing:
				return iconOngoing;
			default:
				return null;
			}
		}

		public void SetColor(Color color) {
			border.color = color;
		}

	}
}