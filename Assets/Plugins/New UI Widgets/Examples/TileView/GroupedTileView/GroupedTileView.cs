namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// GroupedTileView
	/// </summary>
	public class GroupedTileView : ListViewCustom<GroupedTileViewComponent, Photo>
	{
		/// <summary>
		/// Grouped data.
		/// </summary>
		public GroupedPhotos GroupedData = new GroupedPhotos();

		bool isGroupedListViewInited;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isGroupedListViewInited)
			{
				return;
			}

			isGroupedListViewInited = true;

			base.Init();

			GroupedData.GroupComparison = (x, y) => x.Created.CompareTo(y.Created);
			GroupedData.Data = DataSource;

			GroupedData.ItemsPerBlock = 4;
			GroupedData.EmptyGroupItem = new Photo() { IsGroup = true, IsEmpty = true };
			GroupedData.EmptyItem = new Photo() { IsEmpty = true };
		}

		void CalculateItemsPerBlock()
		{
			var size = ScrollRectSize.x;

			var per_block = 1;
			size -= ItemSize.x;

			if (size > 0)
			{
				per_block += Mathf.FloorToInt(size / (ItemSize.x + GetItemSpacingX()));
			}

			GroupedData.ItemsPerBlock = per_block;
		}

		/// <summary>
		/// Sets the need resize.
		/// </summary>
		protected override void SetNeedResize()
		{
			UpdateScrollRectSize();

			CalculateItemsPerBlock();

			base.SetNeedResize();
		}
	}
}