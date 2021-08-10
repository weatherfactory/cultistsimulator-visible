namespace UIWidgets
{
	using System;

	/// <summary>
	/// ListViewString.
	/// </summary>
	public class ListViewString : ListViewCustom<ListViewStringItemComponent, string>
	{
		[NonSerialized]
		bool isListViewStringInited = false;

		/// <summary>
		/// Items comparison.
		/// </summary>
		/// <param name="x">First string.</param>
		/// <param name="y">Second string.</param>
		/// <returns>Comparison result.</returns>
		public virtual int ItemsComparison(string x, string y)
		{
			return UtilitiesCompare.Compare(x, y);
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isListViewStringInited)
			{
				return;
			}

			isListViewStringInited = true;

			base.Init();

			DataSource.Comparison = ItemsComparison;
		}
	}
}