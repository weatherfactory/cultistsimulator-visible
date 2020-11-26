namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the popup.
	/// </summary>
	public class StyleSupportPopup : MonoBehaviour, IStylable
	{
		/// <summary>
		/// The content background.
		/// </summary>
		[SerializeField]
		public Image ContentBackground;

		/// <summary>
		/// The delimiter.
		/// </summary>
		[SerializeField]
		public Image Delimiter;

		/// <summary>
		/// The header close button.
		/// </summary>
		[SerializeField]
		public StyleSupportButtonClose HeaderCloseButton;

		/// <summary>
		/// The close button.
		/// </summary>
		[SerializeField]
		public StyleSupportButton CloseButton;

		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			style.Dialog.ContentBackground.ApplyTo(ContentBackground);
			style.Dialog.Delimiter.ApplyTo(Delimiter);
			style.Dialog.Button.ApplyTo(CloseButton);

			HeaderCloseButton.SetStyle(style);

			return true;
		}
		#endregion
	}
}