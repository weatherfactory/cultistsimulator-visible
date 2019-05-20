using Assets.CS.TabletopUI;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
using Noon;
using UnityEditor;

namespace Assets.Editor
{
	public class ContentValidator
	{
		[MenuItem("Tools/Validate Content Import %#i")]
		private static void ValidateContentImport()
		{
			// Clear the console of previous messages to reduce confusion
			EditorUtils.ClearConsole();

			NoonUtility.Log("-- Checking content files for errors --");
#if MODS
			new Registry().Register(new ModManager(false));
#endif
			var contentImporter = new ContentImporter();
			contentImporter.PopulateCompendium(new Compendium());
			NoonUtility.Log("-- Verification complete --");
		}
	}
}
