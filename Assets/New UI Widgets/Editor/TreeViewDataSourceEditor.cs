namespace UIWidgets
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// TreeView.DataSource editor.
	/// </summary>
	[CustomEditor(typeof(TreeViewDataSource))]
	public class TreeViewDataSourceEditor : Editor
	{
		/// <summary>
		/// Open editor window.
		/// </summary>
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Edit"))
			{
				TreeViewDataSourceWindow.Init();
			}
		}
	}
}