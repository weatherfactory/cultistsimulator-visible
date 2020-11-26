namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// TreeViewSampleMultipleComponent interface.
	/// </summary>
	public interface ITreeViewSampleMultipleComponent
	{
		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		void SetData(ITreeViewSampleItem item);

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration);

		/// <summary>
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>GroupedListViewComponent instance.</returns>
		ITreeViewSampleMultipleComponent IInstance(Transform parent);

		/// <summary>
		/// Return instance to cache.
		/// </summary>
		/// <param name="parent">New parent.</param>
		void Free(Transform parent);
	}
}