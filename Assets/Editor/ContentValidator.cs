using System.Reflection;
using Noon;
using UnityEditor;

namespace Assets.Editor
{
	public class ContentValidator
	{
		[MenuItem("Tools/Validate Content %#v")]
		private static void ValidateContent()
		{
			// Clear the console of previous messages to reduce confusion
			var assembly = Assembly.GetAssembly(typeof(SceneView));
			var type = assembly.GetType("UnityEditor.LogEntries");
			var method = type.GetMethod("Clear");
			if (method != null)
				method.Invoke(new object(), null);

			NoonUtility.Log("-- Checking content files for errors --");
			var contentImporter = new ContentImporter();
			contentImporter.PopulateCompendium(new Compendium());
			NoonUtility.Log("-- Verification complete --");
		}
	}
}
