namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the button close.
	/// </summary>
	public class StyleSupportButtonClose : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Background.
		/// </summary>
		[SerializeField]
		public Image Background;

		/// <summary>
		/// The text.
		/// </summary>
		[SerializeField]
		public GameObject Text;

		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			style.ButtonClose.Background.ApplyTo(Background);
			style.ButtonClose.Text.ApplyTo(Text);

			return true;
		}
		#endregion
	}
}