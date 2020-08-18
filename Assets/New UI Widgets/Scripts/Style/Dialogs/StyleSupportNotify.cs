namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the notifaction.
	/// </summary>
	public class StyleSupportNotify : MonoBehaviour, IStylable
	{
		/// <summary>
		/// The background.
		/// </summary>
		[SerializeField]
		public Image Background;

		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			style.Notify.Background.ApplyTo(Background);

			return true;
		}
		#endregion
	}
}