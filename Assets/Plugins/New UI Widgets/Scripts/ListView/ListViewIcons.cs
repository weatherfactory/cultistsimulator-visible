namespace UIWidgets
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// ListViewIcons.
	/// </summary>
	public class ListViewIcons : ListViewCustom<ListViewIconsItemComponent, ListViewIconsItemDescription>
	{
#pragma warning disable 0809
		/// <summary>
		/// Sort items.
		/// Deprecated. Replaced with DataSource.Comparison.
		/// </summary>
		[Obsolete("Replaced with DataSource.Comparison.")]
		public override bool Sort
		{
			get
			{
				return DataSource.Comparison == ItemsComparison;
			}

			set
			{
				if (value)
				{
					DataSource.Comparison = ItemsComparison;
				}
				else
				{
					DataSource.Comparison = null;
				}
			}
		}
#pragma warning restore 0809

		static string GetItemName(ListViewIconsItemDescription item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			return item.LocalizedName ?? item.Name;
		}

		[NonSerialized]
		bool isListViewIconsInited = false;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isListViewIconsInited)
			{
				return;
			}

			isListViewIconsInited = true;

			base.Init();

#pragma warning disable 0618
			if (base.Sort)
			{
				DataSource.Comparison = ItemsComparison;
			}
#pragma warning restore 0618
		}

		/// <summary>
		/// Items comparison.
		/// </summary>
		/// <param name="x">First item.</param>
		/// <param name="y">Second item.</param>
		/// <returns>Result of the comparison.</returns>
		public static int ItemsComparison(ListViewIconsItemDescription x, ListViewIconsItemDescription y)
		{
			return GetItemName(x).CompareTo(GetItemName(y));
		}

		/// <summary>
		/// Default ListView sort.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <returns>Sorted items.</returns>
		protected IEnumerable<ListViewIconsItemDescription> DefaultSort(IEnumerable<ListViewIconsItemDescription> input)
		{
			var output = new List<ListViewIconsItemDescription>(input);

			output.Sort(ItemsComparison);

			return output;
		}
	}
}