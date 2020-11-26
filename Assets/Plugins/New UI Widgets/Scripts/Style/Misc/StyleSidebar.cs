namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Style for the Sidebar.
	/// </summary>
	[Serializable]
	public class StyleSidebar : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the default background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
		}
#endif
	}
}