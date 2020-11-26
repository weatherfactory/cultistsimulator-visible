namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the scrollbar.
	/// </summary>
	[RequireComponent(typeof(Scrollbar))]
	public class StyleSupportScrollbar : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Style for the main background.
		/// </summary>
		[SerializeField]
		public Image MainBackground;

		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public Image Background;

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
			var scrollbar = GetComponent<Scrollbar>();
			if (scrollbar == null)
			{
				return false;
			}

			var scrollbar_style = (scrollbar.direction == Scrollbar.Direction.LeftToRight || scrollbar.direction == Scrollbar.Direction.RightToLeft)
				? style.HorizontalScrollbar
				: style.VerticalScrollbar;

			SetStyle(scrollbar_style);

			return true;
		}

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="style">Style.</param>
		public virtual void SetStyle(StyleScrollbar style)
		{
			style.MainBackground.ApplyTo(MainBackground);
			style.Background.ApplyTo(Background);
			style.Handle.ApplyTo(Handle);
		}
	}
}