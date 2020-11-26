namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the spinner.
	/// </summary>
	public class StyleSupportSpinner : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Spinner.
		/// </summary>
		[SerializeField]
		public Spinner Spinner;

		/// <summary>
		/// SpinnerFloat.
		/// </summary>
		[SerializeField]
		public SpinnerFloat SpinnerFloat;

		/// <summary>
		/// Background.
		/// </summary>
		[SerializeField]
		public Image Background;

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			if (Spinner != null)
			{
				Spinner.SetStyle(style);
			}

			if (SpinnerFloat != null)
			{
				SpinnerFloat.SetStyle(style);
			}

			style.Spinner.Background.ApplyTo(Background);

			return true;
		}
		#endregion
	}
}