namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TileViewSample.
	/// </summary>
	public class TileViewSample : TileViewCustom<TileViewComponentSample, TileViewItemSample>
	{
		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected override void Awake()
		{
			OnSelect.AddListener(ItemSelected);
		}

		void ItemSelected(int index, ListViewItem component)
		{
			if (component != null)
			{
				// (component as TileViewComponentSample).DoSomething();
			}

			Debug.Log(index + ": " + DataSource[index].Name);
		}
	}
}