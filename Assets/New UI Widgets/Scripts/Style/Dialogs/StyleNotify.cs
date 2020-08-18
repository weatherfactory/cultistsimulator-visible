namespace UIWidgets.Styles
{
	using System;

	/// <summary>
	/// Style for the notifaction.
	/// </summary>
	[Serializable]
	public class StyleNotify : IStyleDefaultValues
	{
		/// <summary>
		/// The background.
		/// </summary>
		public StyleImage Background;

		/// <summary>
		/// The text.
		/// </summary>
		public StyleText Text;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			Text.SetDefaultValues();
		}
#endif
	}
}