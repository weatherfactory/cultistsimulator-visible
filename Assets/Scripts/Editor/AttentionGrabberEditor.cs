using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(AttentionGrabber))]
    [CanEditMultipleObjects]
    public class AttentionGrabberEditor: UnityEditor.Editor
    {
        private SerializedProperty lookAtMe;
        void OnEnable()
        {
            lookAtMe = serializedObject.FindProperty("lookAtMe");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(lookAtMe);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
