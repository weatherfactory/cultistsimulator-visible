namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	/// <summary>
	/// OnChange event for Combobox raised OnHideListView with new selected and new removed items.
	/// </summary>
	public class ComboboxChanges : MonoBehaviour
	{
		/// <summary>
		/// Combobox.
		/// </summary>
		[SerializeField]
		public ComboboxIcons Combobox;

		/// <summary>
		/// Event called when Combobox.ListView closed.
		/// </summary>
		[SerializeField]
		public ComboboxChangesEvent OnChange = new ComboboxChangesEvent();

		List<int> selected;

		bool isShow;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected void Start()
		{
			if (Combobox != null)
			{
				Combobox.OnShowListView.AddListener(ShowList);
				Combobox.OnHideListView.AddListener(HideList);
			}
		}

		void ShowList()
		{
			isShow = true;
			selected = Combobox.ListView.SelectedIndices;
		}

		void HideList()
		{
			if (!isShow)
			{
				return;
			}

			isShow = false;

			var new_selected = Combobox.ListView.SelectedIndices;

			var added = new_selected.Except(selected).ToArray();
			var removed = selected.Except(new_selected).ToArray();

			OnChange.Invoke(added, removed);

			Debug.Log("Selected indices: " + Indices2String(new_selected));
			Debug.Log("New selected indices: " + Indices2String(added));
			Debug.Log("Unselected indices: " + Indices2String(removed));
		}

		static string Indices2String(IList<int> indices)
		{
			return string.Join(", ", indices.Select(x => x.ToString()).ToArray());
		}

		/// <summary>
		/// Destroy this instance.
		/// </summary>
		protected void OnDestroy()
		{
			if (Combobox != null)
			{
				Combobox.OnShowListView.RemoveListener(ShowList);
				Combobox.OnHideListView.RemoveListener(HideList);
			}
		}
	}
}