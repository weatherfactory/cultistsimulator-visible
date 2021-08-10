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
	/// Customized ListView's editor.
	/// </summary>
	[CustomEditor(typeof(ListViewBase), true)]
	public class ListViewCustomBaseEditor : UIWidgetsMonoEditor
	{
		/// <summary>
		/// Is it ListViewCustom?
		/// </summary>
		protected bool IsListViewCustom = false;

		/// <summary>
		/// Is it TreeViewCustom?
		/// </summary>
		protected bool IsTreeViewCustom = false;

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
		protected List<string> Properties = new List<string>()
		{
			"interactable",
			"disableScrollRect",
			"virtualization",
			"listType",
			"customItems",
			"multipleSelect",
			"selectedIndex",
			"direction",
			"defaultItem",
			"Container",
			"scrollRect",
			"allowColoring",

			// other
			"EndScrollDelay",
			"Navigation",
			"NavigationSettings",
			"loopedList",
		};

		/// <summary>
		/// Coloring fields.
		/// </summary>
		protected List<string> ColoringFields = new List<string>()
		{
			"defaultColor",
			"defaultBackgroundColor",
			"highlightedColor",
			"highlightedBackgroundColor",
			"selectedColor",
			"selectedBackgroundColor",
			"disabledColor",
			"FadeDuration",
		};

		/// <summary>
		/// Events.
		/// </summary>
		protected List<string> Events = new List<string>()
		{
			"OnSelect",
			"OnDeselect",
			"OnSelectObject",
			"OnDeselectObject",
			"OnStartScrolling",
			"OnEndScrolling",
		};

		/// <summary>
		/// Exclude properties and events.
		/// </summary>
		protected List<string> Exclude = new List<string>()
		{
			"selectedIndices",
			"sort",
			"KeepSelection",

			// obsolete
			"LimitScrollValue",
		};

		/// <summary>
		/// Visualize navigation.
		/// </summary>
		protected GUIContent NavigationVisualize;

		/// <summary>
		/// ShowNavigation field.
		/// </summary>
		protected static FieldInfo NavigationShow;

		/// <summary>
		/// ShowNavigationKey field.
		/// </summary>
		protected static FieldInfo NavigationShowKey;

		static bool DetectGenericType(object instance, string name)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.FullName.StartsWith(name, StringComparison.Ordinal))
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Fill properties list.
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

			if (Events.Contains(property.name) || Properties.Contains(property.name))
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
			// NavigationVisualize = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");
			if (NavigationShow == null)
			{
				NavigationShow = typeof(UnityEditor.UI.SelectableEditor).GetField("s_ShowNavigation", BindingFlags.NonPublic | BindingFlags.Static);
			}

			if (NavigationShowKey == null)
			{
				NavigationShowKey = typeof(UnityEditor.UI.SelectableEditor).GetField("s_ShowNavigationKey", BindingFlags.NonPublic | BindingFlags.Static);
			}

			FillProperties();

			if (!IsListViewCustom)
			{
				IsListViewCustom = DetectGenericType(serializedObject.targetObject, "UIWidgets.ListViewCustom`2");
			}

			if (!IsTreeViewCustom)
			{
				IsTreeViewCustom = DetectGenericType(serializedObject.targetObject, "UIWidgets.TreeViewCustom`2");
			}

			if (IsTreeViewCustom)
			{
				Properties.Remove("customItems");
				Properties.Remove("selectedIndex");
				Properties.Remove("loopedList");
			}

			if (IsListViewCustom)
			{
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
			}
		}

		/// <summary>
		/// Toggle events block.
		/// </summary>
		protected bool ShowEvents;

		void Upgrade()
		{
			Array.ForEach(targets, x =>
			{
				var lv = x as ListViewBase;
				if (lv != null)
				{
					lv.Upgrade();
				}
			});
		}

		/// <summary>
		/// Draw inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			ValidateTargets();

			Upgrade();

			var list_type = (ListViewType)serializedObject.FindProperty("listType").enumValueIndex;
			var is_tile_view = (list_type == ListViewType.TileViewWithFixedSize) || (list_type == ListViewType.TileViewWithVariableSize);

			if (IsListViewCustom)
			{
				serializedObject.Update();

				SerializedProperties.ForEach(property =>
				{
					if (property.Key == "ItemsEvents")
					{
						return;
					}

					if (is_tile_view && (property.Key == "loopedList"))
					{
						return;
					}

					if (ColoringFields.Contains(property.Key))
					{
						return;
					}

					if (property.Key == "customItems")
					{
						EditorGUILayout.PropertyField(property.Value, new GUIContent("Data Source"), true);
					}
					else if (property.Key == "disableScrollRect")
					{
						EditorGUI.indentLevel += 1;
						EditorGUILayout.PropertyField(property.Value, true);
						EditorGUI.indentLevel -= 1;
					}
					else if (property.Key == "NavigationSettings")
					{
						EditorGUI.indentLevel += 1;

						EditorGUILayout.PropertyField(property.Value, new GUIContent("Settings"), true);
						EditorGUI.BeginChangeCheck();

						/*
						var toggleRect = EditorGUILayout.GetControlRect();
						toggleRect.xMin += EditorGUIUtility.labelWidth;

						var visualize = GUI.Toggle(toggleRect, (bool)NavigationShow.GetValue(null), NavigationVisualize, EditorStyles.miniButton);
						NavigationShow.SetValue(null, visualize);
						if (EditorGUI.EndChangeCheck())
						{
							EditorPrefs.SetBool((string)NavigationShowKey.GetValue(null), visualize);
							SceneView.RepaintAll();
						}
						*/

						EditorGUI.indentLevel -= 1;
					}
					else
					{
						EditorGUILayout.PropertyField(property.Value, true);

						if ((property.Key == "allowColoring") && property.Value.boolValue)
						{
							ColoringFields.ForEach(color_field => EditorGUILayout.PropertyField(SerializedProperties[color_field], true));
						}
					}
				});

				ShowEvents = GUILayout.Toggle(ShowEvents, "Events", EditorStyles.foldout, GUILayout.ExpandWidth(true));

				if (ShowEvents)
				{
					SerializedEvents.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));
				}

				EditorGUI.indentLevel += 1;
				EditorGUILayout.PropertyField(SerializedProperties["ItemsEvents"], new GUIContent("Items Events"), true);
				EditorGUI.indentLevel -= 1;

				serializedObject.ApplyModifiedProperties();

				var show_warning = false;
				Array.ForEach(targets, x =>
				{
					var type = x.GetType();
					var method = type.GetMethod("IsVirtualizationPossible", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
					if (method != null)
					{
						var virtualization = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), x, method);
						show_warning |= !virtualization.Invoke();
					}
				});

				if (show_warning)
				{
					if (is_tile_view || IsTreeViewCustom)
					{
						EditorGUILayout.HelpBox("Virtualization requires specified ScrollRect and Container should have EasyLayout component.", MessageType.Warning);
					}
					else
					{
						EditorGUILayout.HelpBox("Virtualization requires specified ScrollRect and Container should have EasyLayout or Horizontal or Vertical Layout Group component.", MessageType.Warning);
					}
				}
			}
			else
			{
				DrawDefaultInspector();
			}

			ValidateTargets();
		}
	}
}
#endif