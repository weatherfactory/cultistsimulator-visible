namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// ListViewIcons drag support.
	/// </summary>
	[RequireComponent(typeof(ListViewIconsItemComponent))]
	public class ListViewIconsDragSupport : ListViewCustomDragSupport<ListViewIcons, ListViewIconsItemComponent, ListViewIconsItemDescription>
	{
		/// <summary>
		/// Get data from specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		/// <returns>Data.</returns>
		protected override ListViewIconsItemDescription GetData(ListViewIconsItemComponent component)
		{
			return component.Item;
		}

		/// <summary>
		/// Set data for DragInfo component.
		/// </summary>
		/// <param name="data">Data.</param>
		protected override void SetDragInfoData(ListViewIconsItemDescription data)
		{
			DragInfo.SetData(data);
		}
	}
}