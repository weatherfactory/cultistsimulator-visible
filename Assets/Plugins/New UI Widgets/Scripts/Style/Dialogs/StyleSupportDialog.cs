namespace UIWidgets.Styles
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the dialog.
	/// </summary>
	public class StyleSupportDialog : MonoBehaviour, IStylable
	{
		/// <summary>
		/// The title.
		/// </summary>
		[SerializeField]
		public GameObject Title;

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
		/// The buttons.
		/// </summary>
		[SerializeField]
		public List<StyleSupportButton> Buttons;

		/// <summary>
		/// The close button.
		/// </summary>
		[SerializeField]
		public StyleSupportButtonClose CloseButton;

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

			if (Buttons != null)
			{
				Buttons.ForEach(style.Dialog.Button.ApplyTo);
			}

			CloseButton.SetStyle(style);

			return true;
		}
		#endregion
	}
}