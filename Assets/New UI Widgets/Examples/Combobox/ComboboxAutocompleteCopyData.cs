namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Copy data from Combobox to Autocomplete.
	/// </summary>
	[RequireComponent(typeof(Autocomplete))]
	[RequireComponent(typeof(Combobox))]
	public class ComboboxAutocompleteCopyData : MonoBehaviour
	{
		/// <summary>
		/// Copy data and destroy this component.
		/// </summary>
		protected virtual void Update()
		{
			GetComponent<Combobox>().ListView.DataSource = GetComponent<Autocomplete>().DataSource.ToObservableList();
			Destroy(this);
		}
	}
}