using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SetMipMapBiasInSelection : EditorWindow {

	[MenuItem("Texture Tools/Mip Map Bias/Set Selection to -0.5", priority = 60)]
	public static void SetBiasMinushalf() {
		SetMipmapBias(-0.5f);
	}

	static string progressText = "Progress...";

	static void SetMipmapBias(float bias) {
		progressText = "Setting Mip map Bias to " + bias ;
		int i = 0;

		ShowProgressBar(i, Selection.objects.Length);

		foreach (Object obj in Selection.objects) {
			string path = AssetDatabase.GetAssetPath(obj);

			TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;

			if (texImporter != null)
			{
				texImporter.mipmapEnabled = true;
				texImporter.mipMapBias = bias;
				AssetDatabase.ImportAsset(path);
			}

			i++;
			ShowProgressBar(i, Selection.objects.Length);
		}

		EditorUtility.ClearProgressBar();
	}

	static void ShowProgressBar(int i, int numItems)  {
		EditorUtility.DisplayProgressBar(progressText, GetProgressDesc(i, numItems), i / numItems);
	}

	static string GetProgressDesc(int numItem, int numItems) {
		return ("Editing item " + numItem + " /  " + numItems);
	}
}
