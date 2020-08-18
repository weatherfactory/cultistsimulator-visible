#if UNITY_EDITOR
namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UIWidgets.Attributes;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Conditional editor.
	/// Fields displayed only if match attribute conditions.
	/// </summary>
	public class EditorConditional : Editor
	{
		/// <summary>
		/// Fields block data.
		/// </summary>
		protected class FieldsBlock
		{
			/// <summary>
			/// Is block visible.
			/// </summary>
			public bool Visible = false;

			/// <summary>
			/// Block name.
			/// </summary>
			public string Name;

			/// <summary>
			/// Block fields.
			/// </summary>
			public Dictionary<string, SerializedProperty> Fields = new Dictionary<string, SerializedProperty>();
		}

		/// <summary>
		/// Conditions to display field.
		/// </summary>
		protected class DisplayConditions
		{
			/// <summary>
			/// Field name.
			/// </summary>
			public string Name;

			/// <summary>
			/// List of the conditions to display field.
			/// </summary>
			public List<IEditorCondition> Conditions = new List<IEditorCondition>();

			/// <summary>
			/// Initializes a new instance of the <see cref="DisplayConditions"/> class.
			/// </summary>
			/// <param name="name">Field name.</param>
			/// <param name="conditions">List of the conditions to display field.</param>
			public DisplayConditions(string name, List<IEditorCondition> conditions)
			{
				Name = name;
				Conditions = conditions;
			}

			/// <summary>
			/// Check if field can be displayed.
			/// </summary>
			/// <param name="properties">Properties.</param>
			/// <returns>true if field should be displayed; otherwise false.</returns>
			public bool IsValid(Dictionary<string, SerializedProperty> properties)
			{
				foreach (var condition in Conditions)
				{
					if (!properties.ContainsKey(condition.Field))
					{
						Debug.LogWarning(string.Format("Field \"{0}\" referenced to non existing field \"{1}\"; conditional check ignored.", Name, condition.Field));
						continue;
					}

					var property = properties[condition.Field];
					if (!condition.IsValid(property))
					{
						return false;
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Type of the editable object.
		/// </summary>
		protected Type TargetType;

		/// <summary>
		/// Fields blocks.
		/// </summary>
		protected Dictionary<string, FieldsBlock> Blocks = new Dictionary<string, FieldsBlock>();

		/// <summary>
		/// Serialized properties.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedProperties = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// Serialized events.
		/// </summary>
		protected Dictionary<string, SerializedProperty> SerializedEvents = new Dictionary<string, SerializedProperty>();

		/// <summary>
		/// Display conditions for the properties.
		/// </summary>
		protected Dictionary<string, DisplayConditions> PropertyDisplayConditions = new Dictionary<string, DisplayConditions>();

		/// <summary>
		/// Properties.
		/// </summary>
		protected List<string> Fields = new List<string>();

		/// <summary>
		/// Events.
		/// </summary>
		protected List<string> Events = new List<string>();

		/// <summary>
		/// Find fields.
		/// </summary>
		protected virtual void DetectFields()
		{
			var property = serializedObject.GetIterator();
			property.NextVisible(true);
			while (property.NextVisible(false))
			{
				if (IsEvent(property))
				{
					Events.Add(property.name);
				}
				else
				{
					Fields.Add(property.name);
				}

				var condition = GetCondition(property.name);
				PropertyDisplayConditions[property.name] = condition;
			}
		}

		/// <summary>
		/// Get field info from the specified type by name.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="name">Field name.</param>
		/// <returns>Field info.</returns>
		protected static FieldInfo GetField(Type type, string name)
		{
			FieldInfo field;

			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			while ((field = type.GetField(name, flags)) == null && (type = type.BaseType) is Type)
			{
			}

			return field;
		}

		/// <summary>
		/// Get conditions for the specified field.
		/// </summary>
		/// <param name="name">Field name.</param>
		/// <returns>Conditions.</returns>
		protected DisplayConditions GetCondition(string name)
		{
			var field = GetField(TargetType, name);
			var conditions = new List<IEditorCondition>();

			if (field != null)
			{
				var attrs = field.GetCustomAttributes(typeof(IEditorCondition), true);
				foreach (var a in attrs)
				{
					conditions.Add(a as IEditorCondition);
				}
			}
			else
			{
				Debug.LogWarning("Field " + name + " is not found in the class " + Utilities.GetFriendlyTypeName(TargetType));
			}

			return new DisplayConditions(name, conditions);
		}

		/// <summary>
		/// Get block name for the field.
		/// </summary>
		/// <param name="name">Field name.</param>
		/// <returns>Block name.</returns>
		protected string GetBlockName(string name)
		{
			var field = GetField(TargetType, name);
			if (field != null)
			{
				var attrs = field.GetCustomAttributes(typeof(EditorConditionBlockAttribute), true);
				foreach (var a in attrs)
				{
					var attr = a as EditorConditionBlockAttribute;
					if (attr != null)
					{
						return attr.Block;
					}
				}
			}
			else
			{
				Debug.LogWarning("Field " + name + " is not found in the class " + Utilities.GetFriendlyTypeName(TargetType));
			}

			return string.Empty;
		}

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
		/// Init.
		/// </summary>
		protected virtual void OnEnable()
		{
			TargetType = serializedObject.targetObject.GetType();

			DetectFields();

			Fields.ForEach(AddField);

			Events.ForEach(AddEvent);
		}

		/// <summary>
		/// Add property.
		/// </summary>
		/// <param name="name">Property name.</param>
		protected void AddField(string name)
		{
			var property = serializedObject.FindProperty(name);
			if (property != null)
			{
				var block_name = GetBlockName(name);
				if (string.IsNullOrEmpty(block_name))
				{
					SerializedProperties[name] = property;
				}
				else
				{
					GetBlock(block_name).Fields[name] = property;
				}
			}
		}

		/// <summary>
		/// Add event.
		/// </summary>
		/// <param name="name">Event name.</param>
		protected void AddEvent(string name)
		{
			var property = serializedObject.FindProperty(name);
			if (property != null)
			{
				GetBlock("Events").Fields[name] = property;
			}
		}

		/// <summary>
		/// Get block by name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>Block.</returns>
		protected FieldsBlock GetBlock(string name)
		{
			if (!Blocks.ContainsKey(name))
			{
				Blocks[name] = new FieldsBlock() { Name = name, };
			}

			return Blocks[name];
		}

		/// <summary>
		/// Toggle events block.
		/// </summary>
		protected bool ShowEvents;

		/// <summary>
		/// Draw inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			foreach (var property in SerializedProperties)
			{
				ShowProperty(property.Key, property.Value);
			}

			foreach (var block in Blocks.Values)
			{
				block.Visible = GUILayout.Toggle(block.Visible, block.Name, "Foldout", GUILayout.ExpandWidth(true));
				if (block.Visible)
				{
					EditorGUI.indentLevel++;
					foreach (var property in block.Fields)
					{
						ShowProperty(property.Key, property.Value);
					}

					EditorGUI.indentLevel--;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Show property.
		/// </summary>
		/// <param name="name">Property name.</param>
		/// <param name="property">Property.</param>
		protected void ShowProperty(string name, SerializedProperty property)
		{
			var conditions = PropertyDisplayConditions[name];
			var is_visible = conditions.IsValid(SerializedProperties);
			if (is_visible)
			{
				var indent = conditions.Conditions.Count;
				EditorGUI.indentLevel += indent;
				EditorGUILayout.PropertyField(property, true);
				EditorGUI.indentLevel -= indent;
			}
		}
	}
}
#endif