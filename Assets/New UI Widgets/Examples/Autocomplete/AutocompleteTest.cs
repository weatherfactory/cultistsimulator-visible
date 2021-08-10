namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Autocomplete test.
	/// </summary>
	public class AutocompleteTest : MonoBehaviour
	{
		/// <summary>
		/// Autocomplete.
		/// </summary>
		[SerializeField]
		public AutocompleteString Autocomplete;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			// OptionSelected will be called when user select value
			Autocomplete.OnOptionSelectedItem.AddListener(OptionSelected);
		}

		/// <summary>
		/// Process selected text.
		/// </summary>
		/// <param name="text">Selected text.</param>
		protected virtual void OptionSelected(string text)
		{
			// do something with text
			Debug.Log("Autocomplete selected value = " + text);
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			Autocomplete.OnOptionSelectedItem.RemoveListener(OptionSelected);
		}
	}
}