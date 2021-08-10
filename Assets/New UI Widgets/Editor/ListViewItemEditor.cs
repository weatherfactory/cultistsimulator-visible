#if UNITY_EDITOR
namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UIWidgets.Extensions;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Customized ListViewItem's editor.
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ListViewItem), true)]
	public class ListViewItemEditor : UIWidgetsMonoEditor
	{
		/// <summary>
		/// Serialized properties.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedProperties = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// Serialized events.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedEvents = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// Properties.
		/// </summary>
		protected List<string> Properties = new List<string>();

		/// <summary>
		/// Events.
		/// </summary>
		protected List<string> Events = new List<string>();

		/// <summary>
		/// Init.
		/// </summary>
		protected virtual void OnEnable()
		{
			Properties.Clear();
			Events.Clear();
			SerializedProperties.Clear();
			SerializedEvents.Clear();

			var property = serializedObject.GetIterator();
			property.NextVisible(true);
			while (property.NextVisible(false))
			{
				AddProperty(property);
			}

			Events.Sort();

			Properties.ForEach(x => SerializedProperties.Add(x, serializedObject.FindProperty(x)));
			Events.ForEach(x => SerializedEvents.Add(x, serializedObject.FindProperty(x)));
		}

		/// <summary>
		/// Detect generic type.
		/// </summary>
		/// <param name="instance">Object instance.</param>
		/// <param name="baseType">Base generic type.</param>
		/// <returns>true if object of specified type; otherwise false.</returns>
		protected virtual bool DetectGenericType(object instance, Type baseType)
		{
			var type = instance.GetType();
			while (type != null)
			{
				if (type == baseType)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Add property.
		/// </summary>
		/// <param name="property">Property.</param>
		protected void AddProperty(SerializedProperty property)
		{
			if (IsEvent(property))
			{
				Events.Add(property.name);
			}
			else
			{
				Properties.Add(property.name);
			}
		}

		/// <summary>
		/// Is event?
		/// </summary>
		/// <param name="property">Property.</param>
		/// <returns>true if property is event; otherwise false.</returns>
		protected virtual bool IsEvent(SerializedProperty property)
		{
			var object_type = property.serializedObject.targetObject.GetType();
			var property_type = object_type.GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			return typeof(UnityEventBase).IsAssignableFrom(property_type.FieldType);
		}

		/// <summary>
		/// Properties and event to exclude.
		/// </summary>
		protected List<string> exclude = new List<string>()
		{
			"Icon",
			"TextAdapter",
			"Toggle",
			"Indentation",
			"OnNodeExpand",
			"AnimateArrow",
			"NodeOpened",
			"NodeClosed",
			"PaddingPerLevel",
			"SetNativeSize",
		};

		/// <summary>
		/// Toggle events block.
		/// </summary>
		protected bool ShowEvents;

		/// <summary>
		/// Draw inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			ValidateTargets();

			serializedObject.Update();

			var isTreeViewNode = DetectGenericType(serializedObject.targetObject, typeof(TreeViewComponentBase<>));
			if (isTreeViewNode)
			{
				EditorGUILayout.PropertyField(SerializedProperties["Icon"], true);
				EditorGUILayout.PropertyField(SerializedProperties["TextAdapter"], true);
				EditorGUILayout.PropertyField(SerializedProperties["Toggle"], true);
				EditorGUILayout.PropertyField(SerializedProperties["Indentation"], true);

				EditorGUILayout.PropertyField(SerializedProperties["OnNodeExpand"], true);
				EditorGUI.indentLevel++;

				// rotate
				if (SerializedProperties["OnNodeExpand"].enumValueIndex == 0)
				{
					EditorGUILayout.PropertyField(SerializedProperties["AnimateArrow"], true);
				}
				else
				{
					EditorGUILayout.PropertyField(SerializedProperties["NodeOpened"], true);
					EditorGUILayout.PropertyField(SerializedProperties["NodeClosed"], true);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.PropertyField(SerializedProperties["PaddingPerLevel"], true);
				EditorGUILayout.PropertyField(SerializedProperties["SetNativeSize"], true);

				foreach (var property in SerializedProperties)
				{
					if (!exclude.Contains(property.Key))
					{
						EditorGUILayout.PropertyField(property.Value, true);
					}
				}
			}
			else
			{
				SerializedProperties.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));
			}

			EditorGUILayout.BeginVertical();

			ShowEvents = GUILayout.Toggle(ShowEvents, "Events", EditorStyles.foldout, GUILayout.ExpandWidth(true));
			if (ShowEvents)
			{
				SerializedEvents.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));
			}

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();

			ValidateTargets();
		}
	}
}
#endif