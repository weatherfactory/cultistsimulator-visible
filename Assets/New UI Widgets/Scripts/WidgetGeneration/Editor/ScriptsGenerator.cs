#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UIWidgets.Extensions;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Widget scripts generator.
	/// </summary>
	public class ScriptsGenerator : IFormattable
	{
		/// <summary>
		/// Script data.
		/// </summary>
		public class ScriptData
		{
			/// <summary>
			/// Type.
			/// </summary>
			public readonly string Type;

			/// <summary>
			/// Class name.
			/// </summary>
			public readonly string ClassName;

			/// <summary>
			/// Template.
			/// </summary>
			public readonly TextAsset Template;

			/// <summary>
			/// Can create.
			/// </summary>
			public readonly bool CanCreate;

			/// <summary>
			/// Path.
			/// </summary>
			public readonly string Path;

			/// <summary>
			/// Initializes a new instance of the <see cref="ScriptData"/> class.
			/// </summary>
			/// <param name="type">Type.</param>
			/// <param name="classname">Class name.</param>
			/// <param name="template">Template.</param>
			/// <param name="path">Path.</param>
			/// <param name="canCreate">Can script be created?</param>
			public ScriptData(string type, string classname, TextAsset template, string path, bool canCreate)
			{
				Type = type;
				ClassName = classname;
				Template = template;
				Path = path;
				CanCreate = canCreate;
			}
		}

		/// <summary>
		/// Class info.
		/// </summary>
		public ClassInfo Info;

		/// <summary>
		/// Path to save created files.
		/// </summary>
		protected string SavePath;

		/// <summary>
		/// Path to save created editor scripts.
		/// </summary>
		protected string EditorSavePath;

		/// <summary>
		/// Path to save created widgets scripts.
		/// </summary>
		protected string ScriptSavePath;

		/// <summary>
		/// Path to save created prefab.
		/// </summary>
		protected string PrefabSavePath;

		/// <summary>
		/// Scripts templates.
		/// </summary>
		public readonly Dictionary<string, ScriptData> Templates = new Dictionary<string, ScriptData>();

		/// <summary>
		/// Script templates values.
		/// </summary>
		protected Dictionary<string, string> TemplateValues;

		/// <summary>
		/// Script editor templates.
		/// </summary>
		protected HashSet<string> EditorTemplates = new HashSet<string>()
		{
			"MenuOptions",
			"PrefabGenerator",
			"PrefabGeneratorAutocomplete",
			"PrefabGeneratorTable",
			"PrefabGeneratorScene",
		};

		/// <summary>
		/// Prefabs.
		/// </summary>
		protected List<string> Prefabs = new List<string>()
		{
			"ListView",
			"DragInfo",
			"Combobox",
			"ComboboxMultiselect",
			"Table",
			"TileView",
			"TreeView",
			"TreeGraph",
			"PickerListView",
			"PickerTreeView",
			"Autocomplete",
			"AutoCombobox",
		};

		/// <summary>
		/// Allow autocomplete script and widget.
		/// </summary>
		protected bool AllowAutocomplete;

		/// <summary>
		/// Allow table widget.
		/// </summary>
		protected bool AllowTable;

		/// <summary>
		/// Allow test item generation.
		/// </summary>
		protected bool AllowItem;

		/// <summary>
		/// Add script data.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="template">Template.</param>
		/// <param name="canCreate">Can script be created?</param>
		protected void AddScriptData(string type, TextAsset template, bool canCreate = true)
		{
			var classname = type + Info.ShortTypeName;
			var dir = EditorTemplates.Contains(type) ? EditorSavePath : ScriptSavePath;
			var path = dir + Path.DirectorySeparatorChar + classname + ".cs";

			var data = new ScriptData(type, classname, template, path, canCreate);
			Templates[type] = data;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptsGenerator"/> class.
		/// </summary>
		/// <param name="info">Class info.</param>
		/// <param name="path">Path to save created files.</param>
		public ScriptsGenerator(ClassInfo info, string path)
		{
			Info = info;
			SavePath = path + "/Widgets" + info.ShortTypeName;
			EditorSavePath = SavePath + Path.DirectorySeparatorChar + "Editor";
			ScriptSavePath = SavePath + Path.DirectorySeparatorChar + "Scripts";
			PrefabSavePath = SavePath + Path.DirectorySeparatorChar + "Prefabs";

			var templates = ScriptsTemplates.Instance;

			AllowAutocomplete = !string.IsNullOrEmpty(Info.AutocompleteField);
			AllowTable = Info.TextFields.Count > 0;
			AllowItem = Info.ParameterlessConstructor;

			// collections
			AddScriptData("Autocomplete", templates.Autocomplete, AllowAutocomplete);
			AddScriptData("AutoCombobox", templates.AutoCombobox, AllowAutocomplete);
			AddScriptData("Combobox", templates.Combobox);
			AddScriptData("ListView", templates.ListView);
			AddScriptData("ListViewComponent", templates.ListViewComponent);
			AddScriptData("TreeGraph", templates.TreeGraph);
			AddScriptData("TreeGraphComponent", templates.TreeGraphComponent);
			AddScriptData("TreeView", templates.TreeView);
			AddScriptData("TreeViewComponent", templates.TreeViewComponent);

			// collections support
			AddScriptData("Comparers", templates.Comparers);
			AddScriptData("ListViewDragSupport", templates.ListViewDragSupport);
			AddScriptData("ListViewDropSupport", templates.ListViewDropSupport);
			AddScriptData("TreeViewDropSupport", templates.TreeViewDropSupport);
			AddScriptData("TreeViewNodeDragSupport", templates.TreeViewNodeDragSupport);
			AddScriptData("TreeViewNodeDropSupport", templates.TreeViewNodeDropSupport);

			// dialogs
			AddScriptData("PickerListView", templates.PickerListView);
			AddScriptData("PickerTreeView", templates.PickerTreeView);

			// test
			AddScriptData("Test", templates.Test);
			AddScriptData("TestItem", AllowItem ? templates.TestItem : templates.TestItemOff);

			// test
			AddScriptData("MenuOptions", templates.MenuOptions);

			// generators
			AddScriptData("PrefabGenerator", templates.PrefabGenerator);
			AddScriptData("PrefabGeneratorAutocomplete", AllowAutocomplete ? templates.PrefabGeneratorAutocomplete : templates.PrefabGeneratorAutocompleteOff);
			AddScriptData("PrefabGeneratorTable", AllowTable ? templates.PrefabGeneratorTable : templates.PrefabGeneratorTableOff);
			AddScriptData("PrefabGeneratorScene", templates.PrefabGeneratorScene);
		}

		void SetAvailableScripts()
		{
			foreach (var template in Templates)
			{
				if (template.Value.CanCreate)
				{
					Info.Scripts[template.Key] = !File.Exists(template.Value.Path);
				}
			}
		}

		void SetAvailablePrefabs()
		{
			foreach (var prefab in Prefabs)
			{
				if (CanCreateWidget(prefab))
				{
					Info.Prefabs[prefab] = !File.Exists(Prefab2Filename(prefab));
				}
			}
		}

		void SetAvailableScenes()
		{
			Info.Scenes["TestScene"] = !File.Exists(Scene2Filename(Info.ShortTypeName));
		}

		void SetTemplateValues()
		{
			TemplateValues = new Dictionary<string, string>()
			{
				{ "WidgetsNamespace", Info.WidgetsNamespace },
				{ "SourceClassShortName", Info.ShortTypeName },
				{ "SourceClass", Info.FullTypeName },
				{ "AutocompleteField", Info.AutocompleteField },
				{ "ComparersEnum", "ComparersFields" + Info.ShortTypeName },
				{ "Info", UtilitiesEditor.Serialize(Info) },
				{ "Path", UtilitiesEditor.Serialize(SavePath) },
				{ "TextType", UtilitiesEditor.GetFriendlyTypeName(Info.TextFieldType) },
				{ "AutocompleteInput", UtilitiesEditor.GetFriendlyTypeName(Info.InputField) },
				{ "AutocompleteText", UtilitiesEditor.GetFriendlyTypeName(Info.InputText) },
				{ "PrefabsMenuGUID", Info.PrefabsMenuGUID },
			};
		}

		/// <summary>
		/// Generate files.
		/// </summary>
		public void Generate()
		{
			SetAvailableScripts();
			SetAvailablePrefabs();
			SetAvailableScenes();

			OverwriteRequestWindow.Open(this, GenerateScripts);
		}

		void GenerateScripts()
		{
			Directory.CreateDirectory(EditorSavePath);
			Directory.CreateDirectory(ScriptSavePath);
			Directory.CreateDirectory(PrefabSavePath);

			if (Info.Scenes["TestScene"])
			{
				if (!Compatibility.SceneSave())
				{
					EditorUtility.DisplayDialog("Widget Generation", "Please save scene to continue.", "OK");
					return;
				}

				Compatibility.SceneNew();
			}

			var menu = ScriptableObject.CreateInstance<PrefabsMenuGenerated>();
			var menu_path = PrefabSavePath + Path.DirectorySeparatorChar + "PrefabsMenu" + Info.ShortTypeName + ".asset";
			AssetDatabase.CreateAsset(menu, menu_path);
			Info.PrefabsMenuGUID = AssetDatabase.AssetPathToGUID(menu_path);

			SetTemplateValues();

			try
			{
				var progress = 0;
				ProgressbarUpdate(progress);

				foreach (var template in Templates)
				{
					CreateScript(template.Value);

					progress++;
					ProgressbarUpdate(progress);
				}
			}
			catch (Exception)
			{
				EditorUtility.ClearProgressBar();
				throw;
			}

			AssetDatabase.Refresh();
		}

		void ProgressbarUpdate(int progress)
		{
			if (progress < Templates.Count)
			{
				EditorUtility.DisplayProgressBar("Widget Generation", "Step 1. Creating scripts.", progress / (float)Templates.Count);
			}
			else
			{
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>
		/// Is widget can be created?
		/// </summary>
		/// <param name="name">Widget name.</param>
		/// <returns>True if widget can be created; otherwise false.</returns>
		protected virtual bool CanCreateWidget(string name)
		{
			if (name == "Autocomplete")
			{
				return AllowAutocomplete;
			}

			if (name == "AutoCombobox")
			{
				return AllowAutocomplete;
			}

			if (name == "Table")
			{
				return AllowTable;
			}

			return true;
		}

		/// <summary>
		/// Get prefab filename by widget name.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>Filename.</returns>
		public string Prefab2Filename(string type)
		{
			return PrefabSavePath + "/" + type + Info.ShortTypeName + ".prefab";
		}

		/// <summary>
		/// Get prefab filename by widget name.
		/// </summary>
		/// <param name="scene">Scene.</param>
		/// <returns>Filename.</returns>
		public string Scene2Filename(string scene)
		{
			return SavePath + "/" + scene + ".unity";
		}

		/// <summary>
		/// Create script by the specified template.
		/// </summary>
		/// <param name="data">Script data.</param>
		protected virtual void CreateScript(ScriptData data)
		{
			bool can_create;
			if (!Info.Scripts.TryGetValue(data.Type, out can_create))
			{
				return;
			}

			if (!can_create)
			{
				return;
			}

			var code = string.Format(data.Template.text, this);

			File.WriteAllText(data.Path, code);
		}

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (TemplateValues.ContainsKey(format))
			{
				return TemplateValues[format];
			}

			if (format.EndsWith("Class"))
			{
				var key = format.Substring(0, format.Length - "Class".Length);
				if (Templates.ContainsKey(key))
				{
					return Templates[key].ClassName;
				}
			}

			var pos = format.IndexOf("@");
			if (pos != -1)
			{
				return ToStringList(format, formatProvider);
			}

			throw new ArgumentOutOfRangeException("Unsupported format: " + format);
		}

		readonly List<Type> TypesInt = new List<Type>()
		{
			typeof(decimal),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
		};

		readonly List<Type> TypesFloat = new List<Type>()
		{
			typeof(float),
			typeof(double),
		};

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed.")]
		protected string ToStringList(string format, IFormatProvider formatProvider)
		{
			var template = format.Split(new[] { "@" }, 2, StringSplitOptions.None);
			template[1] = template[1].Replace("[", "{").Replace("]", "}");

			switch (template[0])
			{
				case "IfTMProText":
					return Info.IsTMProText ? string.Format(template[1], this) : string.Empty;
				case "!IfTMProText":
					return !Info.IsTMProText ? string.Format(template[1], this) : string.Empty;
				case "IfTMProInputField":
					return Info.IsTMProInputField ? string.Format(template[1], this) : string.Empty;
				case "!IfTMProInputField":
					return !Info.IsTMProInputField ? string.Format(template[1], this) : string.Empty;
				case "IfAutocomplete":
					return AllowAutocomplete ? string.Format(template[1], this) : string.Empty;
				case "!IfAutocomplete":
					return !AllowAutocomplete ? string.Format(template[1], this) : string.Empty;
				case "IfTable":
					return AllowTable ? string.Format(template[1], this) : string.Empty;
				case "!IfTable":
					return !AllowTable ? string.Format(template[1], this) : string.Empty;
				case "Fields":
					return Info.Fields.ToString(template[1], this, formatProvider);
				case "TextFields":
					return Info.TextFields.ToString(template[1], this, formatProvider);
				case "TextFieldFirst":
					return Info.TextFieldFirst.ToString(template[1], this, formatProvider);
				case "TableFieldFirst":
					return Info.TableFieldFirst.ToString(template[1], this, formatProvider);
				case "ImageFields":
					var fields_image = Info.Fields.Where(IsImage).ToList();
					return fields_image.ToString(template[1], this, formatProvider);
				case "ImageFieldsNullable":
					var fields_image_nullable = Info.Fields.Where(IsImageNullable).ToList();
					return fields_image_nullable.ToString(template[1], this, formatProvider);
				case "TreeViewFields":
					var fields_tree = Info.Fields.Where(IsTreeViewField).ToList();
					return fields_tree.ToString(template[1], this, formatProvider);
				case "FieldsString":
					var fields_string = Info.Fields.Where(IsType<string>).ToList();
					return fields_string.ToString(template[1], this, formatProvider);
				case "FieldsStringFirst":
					var fields_string_first = Info.Fields.Where(IsType<string>).Take(1).ToList();
					return fields_string_first.ToString(template[1], this, formatProvider);
				case "FieldsInt":
					var fields_int = Info.Fields.Where(IsInt).ToList();
					return fields_int.ToString(template[1], this, formatProvider);
				case "FieldsFloat":
					var fields_float = Info.Fields.Where(IsFloat).ToList();
					return fields_float.ToString(template[1], this, formatProvider);
				case "FieldsSprite":
					var fields_sprite = Info.Fields.Where(IsType<Sprite>).ToList();
					return fields_sprite.ToString(template[1], this, formatProvider);
				case "FieldsTexture2D":
					var fields_texture = Info.Fields.Where(IsType<Texture2D>).ToList();
					return fields_texture.ToString(template[1], this, formatProvider);
				case "FieldsColor":
					var fields_color = Info.Fields.Where(IsType<Color, Color32>).ToList();
					return fields_color.ToString(template[1], this, formatProvider);
				default:
					throw new ArgumentOutOfRangeException("Unsupported format: " + format);
			}
		}

		/// <summary>
		/// Is field used with TreeViewComponent?
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>true if field used with TreeViewComponent; otherwise false.</returns>
		protected bool IsTreeViewField(ClassField field)
		{
			return field.WidgetFieldName != "TextAdapter" && field.WidgetFieldName != "Icon";
		}

		/// <summary>
		/// Is field is image type?
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>true if field is image type; otherwise false.</returns>
		protected bool IsImage(ClassField field)
		{
			return field.IsImage;
		}

		/// <summary>
		/// Is field is image and null-able type?
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>true if field is image and null-able type; otherwise false.</returns>
		protected bool IsImageNullable(ClassField field)
		{
			return field.IsImage && field.IsNullable;
		}

		/// <summary>
		/// Is field is integer type?
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>true if field is integer type; otherwise false.</returns>
		protected bool IsInt(ClassField field)
		{
			return TypesInt.Contains(field.FieldType);
		}

		/// <summary>
		/// Is field is float type?
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>true if field is float type; otherwise false.</returns>
		protected bool IsFloat(ClassField field)
		{
			return TypesFloat.Contains(field.FieldType);
		}

		/// <summary>
		/// Is field type is the specified type?
		/// </summary>
		/// <typeparam name="T">Type.</typeparam>
		/// <param name="field">Field.</param>
		/// <returns>true if field type is the specified type; otherwise false.</returns>
		protected bool IsType<T>(ClassField field)
		{
			return field.FieldType == typeof(T);
		}

		/// <summary>
		/// Is field type is one of the specified types?
		/// </summary>
		/// <typeparam name="T1">First type.</typeparam>
		/// <typeparam name="T2">Second type.</typeparam>
		/// <param name="field">Field.</param>
		/// <returns>true if field type is on of the specified types; otherwise false.</returns>
		protected bool IsType<T1, T2>(ClassField field)
		{
			return field.FieldType == typeof(T1) || field.FieldType == typeof(T2);
		}
	}
}
#endif