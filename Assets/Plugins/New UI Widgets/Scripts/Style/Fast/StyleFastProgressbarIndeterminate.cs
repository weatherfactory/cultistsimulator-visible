namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Fast style for scrollbar.
	/// </summary>
	[Serializable]
	public class StyleFastProgressbarIndeterminate : IStyleDefaultValues
	{
		/// <summary>
		/// Background.
		/// </summary>
		[SerializeField]
		public StyleRawImage Background;

		/// <summary>
		/// Border.
		/// </summary>
		[SerializeField]
		public StyleImage Border;

		/// <summary>
		/// Mask.
		/// </summary>
		[SerializeField]
		public StyleImage Mask;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			Mask.SetDefaultValues();
			Border.SetDefaultValues();
		}
#endif
	}
}