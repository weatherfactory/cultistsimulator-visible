namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the slider.
	/// </summary>
	[RequireComponent(typeof(Slider))]
	public class StyleSupportSlider : MonoBehaviour, IStylable
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
		public Image Fill;

		/// <summary>
		/// Style for the handle.
		/// </summary>
		[SerializeField]
		public Image Handle;

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			var slider = GetComponent<Slider>();
			if (slider == null)
			{
				return false;
			}

			SetStyle(style.Slider);

			return true;
		}

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="style">Style.</param>
		public virtual void SetStyle(StyleSlider style)
		{
			style.Background.ApplyTo(Background);
			style.Fill.ApplyTo(Fill);
			style.Handle.ApplyTo(Handle);
		}
	}
}