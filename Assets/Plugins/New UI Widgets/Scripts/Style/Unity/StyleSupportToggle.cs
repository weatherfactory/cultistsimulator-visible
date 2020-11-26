namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the toggle.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class StyleSupportToggle : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Style for the main background.
		/// </summary>
		[SerializeField]
		public Image Background;

		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public Image Checkmark;

		/// <summary>
		/// Style for the handle.
		/// </summary>
		[SerializeField]
		public GameObject Label;

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			var slider = GetComponent<Toggle>();
			if (slider == null)
			{
				return false;
			}

			SetStyle(style.Toggle);

			return true;
		}

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="style">Style.</param>
		public virtual void SetStyle(StyleToggle style)
		{
			style.Background.ApplyTo(Background);
			style.Checkmark.ApplyTo(Checkmark);
			style.Label.ApplyTo(Label);
		}
	}
}