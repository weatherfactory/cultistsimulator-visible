namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;

	/// <summary>
	/// ListViewCustom sample.
	/// </summary>
	public class ListViewCustomSample : ListViewCustom<ListViewCustomSampleComponent, ListViewCustomSampleItemDescription>
	{
		readonly Comparison<ListViewCustomSampleItemDescription> itemsComparison = (x, y) => x.Name.CompareTo(y.Name);

		bool isListViewCustomSampleInited = false;

		/// <summary>
		/// Set items comparison.
		/// </summary>
		public override void Init()
		{
			if (isListViewCustomSampleInited)
			{
				return;
			}

			isListViewCustomSampleInited = true;

			base.Init();
			DataSource.Comparison = itemsComparison;
		}
	}
}