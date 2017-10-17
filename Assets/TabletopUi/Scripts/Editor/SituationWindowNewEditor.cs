using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;

[CustomEditor(typeof(SituationWindow))]
public class SituationWindowNewEditor : Editor {

	SituationWindow window;
	List<RecipeSlot> addedSlots = new List<RecipeSlot>();

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		window = target as SituationWindow;

		if (GUILayout.Button("Add Slot")) {
			window.slotManager.AddSlot(BuildSlot());
			window.slotManager.ReorderSlots();
		}
		
		if (GUILayout.Button("Remove Slot") && addedSlots.Count > 0) {
			window.slotManager.RemoveSlot(addedSlots[addedSlots.Count - 1]);
			window.slotManager.ReorderSlots();
			addedSlots.RemoveAt(addedSlots.Count - 1); 
		}
	}



	public virtual RecipeSlot BuildSlot()
	{
		var slot = GameObject.Instantiate(window.slotPrefab);
		addedSlots.Add(slot);
		return slot;    
	}
}
