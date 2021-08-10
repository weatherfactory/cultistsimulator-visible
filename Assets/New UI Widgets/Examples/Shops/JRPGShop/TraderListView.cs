namespace UIWidgets.Examples.Shops
{
	using System;
	using System.Collections.Generic;
	using UIWidgets;

	/// <summary>
	/// TraderListView sort fields.
	/// </summary>
	public enum TraderListViewSortFields
	{
		/// <summary>
		/// Item name.
		/// </summary>
		ItemName,

		/// <summary>
		/// Item available.
		/// </summary>
		ItemAvailable,

		/// <summary>
		/// Price.
		/// </summary>
		Price,

		/// <summary>
		/// Count.
		/// </summary>
		Count,
	}

	/// <summary>
	/// Trader list view.
	/// </summary>
	public class TraderListView : ListViewCustom<TraderListViewComponent, JRPGOrderLine>
	{
		TraderListViewSortFields currentSortField = TraderListViewSortFields.ItemName;

		Dictionary<int, Comparison<JRPGOrderLine>> sortComparers;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			sortComparers = new Dictionary<int, Comparison<JRPGOrderLine>>()
			{
				{ (int)TraderListViewSortFields.ItemName, ItemNameComparer },
				{ (int)TraderListViewSortFields.ItemAvailable, ItemAvailableComparer },
				{ (int)TraderListViewSortFields.Price, PriceComparer },
				{ (int)TraderListViewSortFields.Count, CountComparer },
			};

			base.Init();
		}

		/// <summary>
		/// Toggle sort.
		/// </summary>
		/// <param name="field">Sort field.</param>
		public void ToggleSort(TraderListViewSortFields field)
		{
			if (field == currentSortField)
			{
				DataSource.Reverse();
			}
			else if (sortComparers.ContainsKey((int)field))
			{
				currentSortField = field;

				DataSource.Sort(sortComparers[(int)field]);
			}
		}

		#region used in Button.OnClick()

		/// <summary>
		/// Sort by Item name.
		/// </summary>
		public void SortByItemName()
		{
			ToggleSort(TraderListViewSortFields.ItemName);
		}

		/// <summary>
		/// Sort by Item available.
		/// </summary>
		public void SortByItemAvailable()
		{
			ToggleSort(TraderListViewSortFields.ItemAvailable);
		}

		/// <summary>
		/// Sort by Price.
		/// </summary>
		public void SortByPrice()
		{
			ToggleSort(TraderListViewSortFields.Price);
		}

		/// <summary>
		/// Sort by Count.
		/// </summary>
		public void SortByCount()
		{
			ToggleSort(TraderListViewSortFields.Count);
		}
		#endregion

		#region Items comparers

		/// <summary>
		/// Item name comparer.
		/// </summary>
		/// <param name="x">First JRPGOrderLine.</param>
		/// <param name="y">Second JRPGOrderLine.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		protected static int ItemNameComparer(JRPGOrderLine x, JRPGOrderLine y)
		{
			return UtilitiesCompare.Compare(x.Item.Name, y.Item.Name);
		}

		/// <summary>
		/// Item available comparer.
		/// </summary>
		/// <param name="x">First JRPGOrderLine.</param>
		/// <param name="y">Second JRPGOrderLine.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		protected static int ItemAvailableComparer(JRPGOrderLine x, JRPGOrderLine y)
		{
			if (x.Item.Count == y.Item.Count)
			{
				return 0;
			}

			if (x.Item.Count == -1)
			{
				return 1;
			}

			if (y.Item.Count == -1)
			{
				return -1;
			}

			return x.Item.Count.CompareTo(y.Item.Count);
		}

		/// <summary>
		/// Price comparer.
		/// </summary>
		/// <param name="x">First JRPGOrderLine.</param>
		/// <param name="y">Second JRPGOrderLine.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		protected static int PriceComparer(JRPGOrderLine x, JRPGOrderLine y)
		{
			return x.Price.CompareTo(y.Price);
		}

		/// <summary>
		/// Count comparer.
		/// </summary>
		/// <param name="x">First JRPGOrderLine.</param>
		/// <param name="y">Second JRPGOrderLine.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		protected static int CountComparer(JRPGOrderLine x, JRPGOrderLine y)
		{
			return x.Count.CompareTo(y.Count);
		}
		#endregion
	}
}