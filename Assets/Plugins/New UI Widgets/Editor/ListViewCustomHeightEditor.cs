namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEditor;

	/// <summary>
	/// ListViewCustomHeight editor.
	/// </summary>
	public class ListViewCustomHeightEditor : ListViewCustomEditor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ListViewCustomHeightEditor"/> class.
		/// Constructor.
		/// </summary>
		public ListViewCustomHeightEditor()
		{
			Properties.Add("ForceAutoHeightCalculation");
		}
	}
}