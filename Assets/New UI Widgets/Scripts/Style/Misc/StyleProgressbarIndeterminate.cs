namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Style for the indeterminate progressbar.
	/// </summary>
	[Serializable]
	public class StyleProgressbarIndeterminate : IStyleDefaultValues
	{
		/// <summary>
		/// Progressbar texture.
		/// </summary>
		[SerializeField]
		public StyleRawImage Texture;

		/// <summary>
		/// Progressbar mask.
		/// </summary>
		[SerializeField]
		public StyleImage Mask;

		/// <summary>
		/// Progressbar border.
		/// </summary>
		[SerializeField]
		public StyleImage Border;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Texture.SetDefaultValues();
			Mask.SetDefaultValues();
			Border.SetDefaultValues();
		}
#endif
	}
}