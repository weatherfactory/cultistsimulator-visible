namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// ListView sample. Selected item can be deselected.
	/// </summary>
	[RequireComponent(typeof(ListViewBase))]
	public class ListViewDeselect : MonoBehaviour
	{
		ListViewBase listView;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected void Start()
		{
			listView = GetComponent<ListViewBase>();
			listView.MultipleSelect = true;
			listView.OnSelect.AddListener(SelectListener);
			DeselectAllExceptLast();
		}

		void SelectListener(int index, ListViewItem component)
		{
			DeselectAllExceptLast();
		}

		void DeselectAllExceptLast()
		{
			var indices = listView.SelectedIndices;
			for (int i = 0; i < (indices.Count - 1); i++)
			{
				listView.Deselect(indices[i]);
			}
		}

		/// <summary>
		/// Destroy this instance.
		/// </summary>
		protected void OnDestroy()
		{
			if (listView != null)
			{
				listView.OnSelect.RemoveListener(SelectListener);
			}
		}
	}
}