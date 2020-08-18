namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Disable Combobox input.
	/// </summary>
	[RequireComponent(typeof(Combobox))]
	public class ComboboxDisableDefaultInput : MonoBehaviour
	{
		/// <summary>
		/// Disable combobox input.
		/// </summary>
		protected virtual void Start()
		{
			// disable default combobox input, this also disable input field
			GetComponent<Combobox>().Editable = false;

			// enable input field back
			var adapter = GetComponent<InputFieldAdapter>();
			if (adapter != null)
			{
				adapter.interactable = false;
			}
			else
			{
				var input = GetComponent<InputField>();
				if (input != null)
				{
					input.interactable = true;
				}
			}
		}
	}
}