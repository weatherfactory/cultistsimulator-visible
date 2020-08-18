namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the combobox.
	/// </summary>
	public class StyleSupportCombobox : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Combobox.
		/// </summary>
		[SerializeField]
		public GameObject Combobox;

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			style.Combobox.Background.ApplyTo(GetComponent<Image>());

			if ((Combobox != null) && (Combobox.GetInstanceID() != gameObject.GetInstanceID()))
			{
				var stylable = Compatibility.GetComponent<IStylable>(Combobox);

				if (stylable != null)
				{
					stylable.SetStyle(style);
				}
			}

			return true;
		}
		#endregion
	}
}