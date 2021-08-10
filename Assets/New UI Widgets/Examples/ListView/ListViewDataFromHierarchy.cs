namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Create ListView data from hierarchy.
	/// </summary>
	public class ListViewDataFromHierarchy
	{
		/// <summary>
		/// ListView.
		/// </summary>
		public ListViewIcons ListView;

		/// <summary>
		/// Test.
		/// </summary>
		public void Test()
		{
			ListView.DataSource = Hierarchy2Data(ListView.Container);
		}

		ObservableList<ListViewIconsItemDescription> Hierarchy2Data(Transform source)
		{
			var data = new ObservableList<ListViewIconsItemDescription>();

			foreach (Transform child in source)
			{
				var is_default_item = child.gameObject.GetInstanceID() == ListView.DefaultItem.gameObject.GetInstanceID();
				if (is_default_item)
				{
					// ignore DefaultItem
					continue;
				}

				// convert gameobject to data
				data.Add(new ListViewIconsItemDescription() { Name = child.name, });

				// destroy gameobject
				Object.Destroy(child.gameObject);
			}

			return data;
		}
	}
}