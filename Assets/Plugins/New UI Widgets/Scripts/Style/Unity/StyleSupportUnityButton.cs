namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the Unity button.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class StyleSupportUnityButton : MonoBehaviour, IStylable
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
			style.Button.Background.ApplyTo(Background);
			style.Button.Text.ApplyTo(Text);

			return true;
		}

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="style">Style.</param>
		public virtual void SetStyle(StyleUnityButton style)
		{
			style.Background.ApplyTo(Background);
			style.Text.ApplyTo(Text);
		}
		#endregion
	}
}