namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Style for the Table.
	/// </summary>
	[Serializable]
	public class StyleTable : IStyleDefaultValues
	{
		/// <summary>
		/// Border color.
		/// </summary>
		[SerializeField]
		public StyleImage Border;

		/// <summary>
		/// Background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

		/// <summary>
		/// Heade text.
		/// </summary>
		[SerializeField]
		public StyleText HeaderText;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			HeaderText.SetDefaultValues();
		}
#endif
	}
}