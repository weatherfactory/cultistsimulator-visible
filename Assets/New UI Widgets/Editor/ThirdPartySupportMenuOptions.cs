namespace UIWidgets
{
	using System.IO;
	using UnityEditor;

	/// <summary>
	/// Menu options to toggle third party packages support.
	/// </summary>
	[InitializeOnLoad]
	public static class ThirdPartySupportMenuOptions
	{
		static ThirdPartySupportMenuOptions()
		{
			FixAssemblyDefinitions();
#if UNITY_2017_4_OR_LATER
			EditorApplication.projectChanged += FixAssemblyDefinitions;
#endif
		}

#if UNITY_5_6_OR_NEWER
		#region DataBindSupport
		const string DataBindSupport = "UIWIDGETS_DATABIND_SUPPORT";

		/// <summary>
		/// Is Data Bind for Unity installed?
		/// </summary>
		public static bool DataBindInstalled
		{
			get
			{
				return Utilities.GetType("Slash.Unity.DataBind.Core.Presentation.DataProvider") != null;
			}
		}

		/// <summary>
		/// Enable DataBind support.
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Enable Data Bind support", false, 1000)]
#endif
		public static void EnableDataBindSupport()
		{
			if (CanEnableDataBindSupport())
			{
				var root = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(ReferenceGUID.ScriptsFolder));

				var current_path = AssetDatabase.GUIDToAssetPath(ReferenceGUID.DataBindFolder);
				var new_path = root + "/" + Path.GetFileName(current_path);
				if (current_path != new_path)
				{
					AssetDatabase.MoveAsset(current_path, new_path);
				}

				ScriptingDefineSymbols.Add(DataBindSupport);

				ScriptsRecompile.SetStatus("DataBind", ScriptsRecompile.StatusSymbolsAdded);

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// Can enable DataBind support?
		/// </summary>
		/// <returns>true if DataBind installed and support not enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Enable Data Bind support", true, 1000)]
#endif
		public static bool CanEnableDataBindSupport()
		{
			if (!DataBindInstalled)
			{
				return false;
			}

			return !ScriptingDefineSymbols.Contains(DataBindSupport);
		}

		/// <summary>
		/// Disable DataBind support.
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Disable Data Bind support", false, 1010)]
#endif
		public static void DisableDataBindSupport()
		{
			if (CanDisableDataBindSupport())
			{
				var root = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(ReferenceGUID.ScriptsFolder));

				var current_path = AssetDatabase.GUIDToAssetPath(ReferenceGUID.DataBindFolder);
				var new_path = root + "/Scripts/ThirdPartySupport/" + Path.GetFileName(current_path);
				if (current_path != new_path)
				{
					AssetDatabase.MoveAsset(current_path, new_path);
				}

				ScriptingDefineSymbols.Remove(DataBindSupport);

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// Can disable DataBind support?
		/// </summary>
		/// <returns>true if DataBind support enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Disable Data Bind support", true, 1010)]
#endif
		public static bool CanDisableDataBindSupport()
		{
			return ScriptingDefineSymbols.Contains(DataBindSupport);
		}

		/// <summary>
		/// Recompile DataBind support package?
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Recompile Data Bind support", false, 1015)]
#endif
		public static void RecompileDataBindSupport()
		{
			ScriptsRecompile.SetStatus("DataBind", ScriptsRecompile.StatusSymbolsAdded);
		}

		/// <summary>
		/// Can recompile DataBind support package?
		/// </summary>
		/// <returns>true if DataBind support enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Recompile Data Bind support", true, 1015)]
#endif
		public static bool CanRecompileDataBindSupport()
		{
			return ScriptingDefineSymbols.Contains(DataBindSupport);
		}
		#endregion
#endif

		#region TMProSupport

		const string TMProSupport = "UIWIDGETS_TMPRO_SUPPORT";

		const string TMProAssemblies = "l:UiwidgetsTMProRequiredAssemblyDefinition";

		const string TMProPackage = "Unity.TextMeshPro";

		/// <summary>
		/// Is TextMeshPro installed?
		/// </summary>
		public static bool TMProInstalled
		{
			get
			{
				return !string.IsNullOrEmpty(Compatibility.GetTMProVersion());
			}
		}

		/// <summary>
		/// Enable DataBind Support.
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Enable TextMeshPro support", false, 1020)]
#endif
		public static void EnableTMProSupport()
		{
			if (CanEnableTMProSupport())
			{
				ScriptingDefineSymbols.Add(TMProSupport);
				AssemblyDefinitionsEditor.Add(TMProAssemblies, TMProPackage);

				ScriptsRecompile.SetStatus("TMPro", ScriptsRecompile.StatusSymbolsAdded);

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// Can enable TMPro support?
		/// </summary>
		/// <returns>true if TMPro installed and support not enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Enable TextMeshPro support", true, 1020)]
#endif
		public static bool CanEnableTMProSupport()
		{
			if (!TMProInstalled)
			{
				return false;
			}

			return !ScriptingDefineSymbols.Contains(TMProSupport);
		}

		/// <summary>
		/// Disable TMPro support.
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Disable TextMeshPro support", false, 1030)]
#endif
		public static void DisableTMProSupport()
		{
			if (CanDisableTMProSupport())
			{
				ScriptingDefineSymbols.Remove(TMProSupport);
				AssemblyDefinitionsEditor.Remove(TMProAssemblies, TMProPackage);

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// Can disable TMPro support?
		/// </summary>
		/// <returns>true if TMPro support enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Disable TextMeshPro support", true, 1030)]
#endif
		public static bool CanDisableTMProSupport()
		{
			return ScriptingDefineSymbols.Contains(TMProSupport);
		}

		/// <summary>
		/// Recompile TMPro support package.
		/// </summary>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Recompile TextMeshPro support", false, 1050)]
#endif
		public static void RecompileTMProSupport()
		{
			ScriptsRecompile.SetStatus("TMPro", ScriptsRecompile.StatusSymbolsAdded);
		}

		/// <summary>
		/// Can recompile TMPro support package?
		/// </summary>
		/// <returns>true if TMPro support enabled; otherwise false.</returns>
#if !UNITY_2018_3_OR_NEWER
		[MenuItem("Edit/Project Settings/New UI Widgets/Recompile TextMeshPro support", true, 1050)]
#endif
		public static bool CanRecompileTMProSupport()
		{
			return ScriptingDefineSymbols.Contains(TMProSupport);
		}
		#endregion

		/// <summary>
		/// Fix assembly definitions after package was re-imported.
		/// </summary>
		[UnityEditor.Callbacks.DidReloadScripts]
		public static void FixAssemblyDefinitions()
		{
			if (!ScriptingDefineSymbols.Contains(TMProSupport))
			{
				return;
			}

			if (AssemblyDefinitionsEditor.Contains(TMProAssemblies, TMProPackage))
			{
				return;
			}

			AssemblyDefinitionsEditor.Add(TMProAssemblies, TMProPackage);
			ScriptsRecompile.SetStatus("TMPro", ScriptsRecompile.StatusSymbolsAdded);
			AssetDatabase.Refresh();
		}
	}
}