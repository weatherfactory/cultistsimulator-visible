namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;

	/// <summary>
	/// Scripting Define Symbols.
	/// </summary>
	public static class ScriptingDefineSymbols
	{
		static List<BuildTargetGroup> GetTargets()
		{
			var targets = new List<BuildTargetGroup>();
			foreach (var v in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				targets.Add((BuildTargetGroup)v);
			}

			return targets;
		}

		/// <summary>
		/// Add scripting define symbols.
		/// </summary>
		/// <param name="symbol">Symbol to add.</param>
		public static void Add(string symbol)
		{
			foreach (var target in GetTargets())
			{
				var symbols = Symbols(target);

				if (symbols.Contains(symbol))
				{
					continue;
				}

				symbols.Add(symbol);

				Save(symbols, target);
			}
		}

		/// <summary>
		/// Remove scripting define symbols.
		/// </summary>
		/// <param name="symbol">Symbol to remove.</param>
		public static void Remove(string symbol)
		{
			foreach (var target in GetTargets())
			{
				var symbols = Symbols(target);

				if (!symbols.Contains(symbol))
				{
					continue;
				}

				symbols.Remove(symbol);

				Save(symbols, target);
			}
		}

		/// <summary>
		/// Get scripting define symbols.
		/// </summary>
		/// <returns>Scripting define symbols.</returns>
		[System.Obsolete("Replaced with Symbols(BuildTargetGroup target).")]
		public static HashSet<string> All()
		{
			var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

			return new HashSet<string>(symbols.Split(';'));
		}

		/// <summary>
		/// Get scripting define symbols.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <returns>Scripting define symbols.</returns>
		public static HashSet<string> Symbols(BuildTargetGroup target)
		{
			var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

			return new HashSet<string>(symbols.Split(';'));
		}

		/// <summary>
		/// Check if symbol defined in scripting define symbols.
		/// </summary>
		/// <param name="symbol">Symbol.</param>
		/// <returns>True if symbol defined; otherwise false.</returns>
		public static bool Contains(string symbol)
		{
			return Symbols(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(symbol);
		}

		static void Save(HashSet<string> symbols, BuildTargetGroup target)
		{
			var arr = new string[symbols.Count];
			symbols.CopyTo(arr);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", arr));
			AssetDatabase.Refresh();
		}
	}
}