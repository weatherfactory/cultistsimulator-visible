namespace UIWidgets.Styles
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Menu options.
	/// </summary>
	public static class StyleMenuOptions
	{
		/// <summary>
		/// Creates the style.
		/// </summary>
		[MenuItem("Assets/Create/New UI Widgets/Style", false)]
		public static void CreateStyle()
		{
			var folder = "Assets";
			if (Selection.activeObject != null)
			{
				folder = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (!System.IO.Directory.Exists(folder))
				{
					folder = System.IO.Path.GetDirectoryName(folder);
				}
			}

			var path = folder + "/New UI Widgets Style.asset";
			var file = AssetDatabase.GenerateUniqueAssetPath(path);
			var style = ScriptableObject.CreateInstance<Style>();

			AssetDatabase.CreateAsset(style, file);
			EditorUtility.SetDirty(style);
			AssetDatabase.SaveAssets();

			style.SetDefaultValues();
			EditorUtility.SetDirty(style);
			AssetDatabase.SaveAssets();

			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(file);
		}

		/// <summary>
		/// Apply the default style.
		/// </summary>
		[MenuItem("GameObject/UI/New UI Widgets/Apply default style", false, 10)]
		public static void ApplyDefaultStyle()
		{
			var style = PrefabsMenu.Instance.DefaultStyle;
			if (style == null)
			{
				Debug.LogWarning("Default style not found.");
				return;
			}

			Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Apply style");
			style.ApplyTo(Selection.activeGameObject);
		}

		/// <summary>
		/// Check is the default style exists.
		/// </summary>
		/// <returns><c>true</c>, if default style is exists, <c>false</c> otherwise.</returns>
		[MenuItem("GameObject/UI/New UI Widgets/Apply default style", true, 10)]
		public static bool DefaultStyleIsExists()
		{
			return Selection.activeGameObject != null;
		}
	}
}