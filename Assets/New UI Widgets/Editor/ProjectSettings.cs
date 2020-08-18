#if UNITY_2018_3_OR_NEWER
namespace UIWidgets
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Project settings.
	/// </summary>
	[InitializeOnLoad]
	public static class ProjectSettings
	{
		static ProjectSettings()
		{
			ThirdPartySupportMenuOptions.FixAssemblyDefinitions();
		}

		class Styles
		{
			private Styles()
			{
			}

			/// <summary>
			/// TextMeshPro label.
			/// </summary>
			public static GUIContent TMProLabel = new GUIContent("TextMeshPro Support");

			/// <summary>
			/// DataBind label.
			/// </summary>
			public static GUIContent DataBindLabel = new GUIContent("Data Bind for Unity Support");
		}

		static string TMProStatus
		{
			get
			{
				if (!ThirdPartySupportMenuOptions.TMProInstalled)
				{
					return "TextMeshPro not installed.";
				}

				if (ThirdPartySupportMenuOptions.CanDisableTMProSupport())
				{
					return "Enabled";
				}

				return "Disabled";
			}
		}

		static void TMProBlock()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Styles.TMProLabel, GUILayout.Width(170));
			EditorGUILayout.LabelField(TMProStatus, GUILayout.Width(200));

			if (ThirdPartySupportMenuOptions.TMProInstalled)
			{
				if (ThirdPartySupportMenuOptions.CanDisableTMProSupport())
				{
					if (GUILayout.Button("Disable"))
					{
						ThirdPartySupportMenuOptions.DisableTMProSupport();
					}

					if (GUILayout.Button("Recompile"))
					{
						ThirdPartySupportMenuOptions.RecompileTMProSupport();
					}
				}
				else
				{
					if (GUILayout.Button("Enable"))
					{
						ThirdPartySupportMenuOptions.EnableTMProSupport();
					}
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		static string DataBindStatus
		{
			get
			{
				if (!ThirdPartySupportMenuOptions.DataBindInstalled)
				{
					return "Data Bind for Unity not installed.";
				}

				if (ThirdPartySupportMenuOptions.CanDisableDataBindSupport())
				{
					return "Enabled";
				}

				return "Disabled";
			}
		}

		static void DataBindBlock()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Styles.DataBindLabel, GUILayout.Width(170));
			EditorGUILayout.LabelField(DataBindStatus, GUILayout.Width(200));

			if (ThirdPartySupportMenuOptions.DataBindInstalled)
			{
				if (ThirdPartySupportMenuOptions.CanDisableDataBindSupport())
				{
					if (GUILayout.Button("Disable"))
					{
						ThirdPartySupportMenuOptions.DisableDataBindSupport();
					}

					if (GUILayout.Button("Recompile"))
					{
						ThirdPartySupportMenuOptions.RecompileDataBindSupport();
					}
				}
				else
				{
					if (GUILayout.Button("Enable"))
					{
						ThirdPartySupportMenuOptions.EnableDataBindSupport();
					}
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Create settings provider.
		/// </summary>
		/// <returns>Settings provider.</returns>
		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider()
		{
			var provider = new SettingsProvider("Project/New UI Widgets", SettingsScope.Project)
			{
				guiHandler = (searchContext) =>
				{
					TMProBlock();
					DataBindBlock();
				},

				keywords = SettingsProvider.GetSearchKeywordsFromGUIContentProperties<Styles>(),
			};

			return provider;
		}
	}
}
#endif