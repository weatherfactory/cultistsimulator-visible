namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// Interface for GroupedListViewComponent.
	/// </summary>
	public interface IGroupedListViewComponent
	{
		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		void SetData(IGroupedListItem item);

		/// <summary>
		/// Graphics coloring.
		/// </summary>
		/// <param name="foreground">Foreground color.</param>
		/// <param name="background">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		void GraphicsColoring(Color foreground, Color background, float fadeDuration);

		/// <summary>
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>GroupedListViewComponent instance.</returns>
		IGroupedListViewComponent IInstance(Transform parent);

		/// <summary>
		/// Return instance to cache.
		/// </summary>
		/// <param name="parent">New parent.</param>
		void Free(Transform parent);
	}
}