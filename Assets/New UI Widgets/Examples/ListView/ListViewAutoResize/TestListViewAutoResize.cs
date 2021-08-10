namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test ListViewAutoResize.
	/// </summary>
	[RequireComponent(typeof(ListViewIcons))]
	public class TestListViewAutoResize : MonoBehaviour
	{
		ListViewIcons listView;

		/// <summary>
		/// ListView.
		/// </summary>
		protected ListViewIcons ListView
		{
			get
			{
				if (listView == null)
				{
					listView = GetComponent<ListViewIcons>();
				}

				return listView;
			}
		}

		/// <summary>
		/// Test add.
		/// </summary>
		public void TestAdd()
		{
			ListView.DataSource.Add(new ListViewIconsItemDescription() { Name = "test " + ListView.DataSource.Count });
		}

		/// <summary>
		/// Test delete.
		/// </summary>
		public void TestDelete()
		{
			var c = ListView.DataSource.Count;
			if (c > 0)
			{
				ListView.DataSource.RemoveAt(c - 1);
			}
		}
	}
}