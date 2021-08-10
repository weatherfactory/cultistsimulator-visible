namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Test Popup.
	/// </summary>
	public class PopupTest : MonoBehaviour
	{
		/// <summary>
		/// Popup template.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("popup")]
		protected Popup PopupTemplate;

		Popup currentPopup;

		ListViewInt currentListView;

		/// <summary>
		/// Show picker.
		/// </summary>
		public void ShowPicker()
		{
			currentPopup = PopupTemplate.Clone();
			currentPopup.Show();
			currentListView = currentPopup.GetComponentInChildren<ListViewInt>();

			// fill list with values
			currentListView.DataSource = UtilitiesCollections.CreateList(100, x => x);

			// deselect
			currentListView.SelectedIndex = -1;
			currentListView.OnSelectObject.AddListener(Callback);
		}

		void Callback(int index)
		{
			// do something with value
			Debug.Log(currentListView.DataSource[index]);

			currentListView.OnSelectObject.RemoveListener(Callback);
			currentPopup.Close();
		}
	}
}