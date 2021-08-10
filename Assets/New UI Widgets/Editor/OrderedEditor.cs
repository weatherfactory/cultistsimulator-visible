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
	/// Ordered editor - events will be displayed last.
	/// </summary>
	public class OrderedEditor : UIWidgetsMonoEditor
	{
		/// <summary>
		/// The serialized properties.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedProperties = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// The serialized events.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedEvents = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// Serialized cursors.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedCursors = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// The properties to exclude.
		/// </summary>
		protected List<string> Exclude = new List<string>() { };

		/// <summary>
		/// The properties names.
		/// </summary>
		protected List<string> Properties = new List<string>();

		/// <summary>
		/// The properties names.
		/// </summary>
		protected List<string> Events = new List<string>();

		/// <summary>
		/// Cursors.
		/// </summary>
		protected List<string> Cursors = new List<string>();

		/// <summary>
		/// Init.
		/// </summary>
		protected virtual void OnEnable()
		{
			FillProperties();

			Properties.ForEach(x =>
			{
				var property = serializedObject.FindProperty(x);
				if (property != null)
				{
					SerializedProperties[x] = property;
				}
			});

			Events.ForEach(x =>
			{
				var property = serializedObject.FindProperty(x);
				if (property != null)
				{
					SerializedEvents[x] = property;
				}
			});

			Cursors.ForEach(x =>
			{
				var property = serializedObject.FindProperty(x);
				if (property != null)
				{
					SerializedCursors[x] = property;
				}
			});
		}

		/// <summary>
		/// Fills the properties list.
		/// </summary>
		protected virtual void FillProperties()
		{
			var property = serializedObject.GetIterator();
			property.NextVisible(true);
			while (property.NextVisible(false))
			{
				AddProperty(property);
			}
		}

		/// <summary>
		/// Add property.
		/// </summary>
		/// <param name="property">Property.</param>
		protected void AddProperty(SerializedProperty property)
		{
			if (Exclude.Contains(property.name))
			{
				return;
			}

			if (Properties.Contains(property.name))
			{
				return;
			}

			if (Events.Contains(property.name))
			{
				return;
			}

			if (Cursors.Contains(property.name))
			{
				return;
			}

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
		/// Toggle cursors block.
		/// </summary>
		protected bool ShowCursors;

		/// <summary>
		/// Is it event?
		/// </summary>
		/// <param name="property">Property</param>
		/// <returns>true if property is event; otherwise false.</returns>
		protected virtual bool IsEvent(SerializedProperty property)
		{
			var object_type = property.serializedObject.targetObject.GetType();
			var property_type = object_type.GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (property_type == null)
			{
				return false;
			}

			return typeof(UnityEventBase).IsAssignableFrom(property_type.FieldType);
		}

		/// <summary>
		/// Draw inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			ValidateTargets();

			serializedObject.Update();

			SerializedProperties.ForEach(x => EditorGUILayout.PropertyField(x.Value));

			EditorGUILayout.BeginVertical();

			ShowCursors = GUILayout.Toggle(ShowCursors, "Cursors", EditorStyles.foldout, GUILayout.ExpandWidth(true));
			if (ShowCursors)
			{
				SerializedCursors.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));
			}

			EditorGUILayout.EndVertical();

			SerializedEvents.ForEach(x => EditorGUILayout.PropertyField(x.Value));

			serializedObject.ApplyModifiedProperties();

			ValidateTargets();
		}
	}
}
#endif