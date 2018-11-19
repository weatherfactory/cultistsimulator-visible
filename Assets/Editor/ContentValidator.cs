using Noon;
using UnityEditor;

namespace Assets.Editor
{
	public class ContentValidator
	{
		[MenuItem("Tools/Validate Content Import %#v")]
		private static void ValidateContentImport()
		{
			// Clear the console of previous messages to reduce confusion
			EditorUtils.ClearConsole();

			NoonUtility.Log("-- Checking content files for errors --");
			var contentImporter = new ContentImporter();
			contentImporter.PopulateCompendium(new Compendium());
			NoonUtility.Log("-- Verification complete --");
		}
	}
}
