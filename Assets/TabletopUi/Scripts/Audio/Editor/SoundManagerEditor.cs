using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		if (GUILayout.Button("Sort")) {
			var soundManager = target as SoundManager;
			soundManager.SortSounds();
			UnityEditor.EditorUtility.SetDirty(target);
		}
	}

}
