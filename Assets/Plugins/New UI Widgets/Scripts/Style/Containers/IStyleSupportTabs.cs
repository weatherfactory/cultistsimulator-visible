namespace UIWidgets.Styles
{
	/// <summary>
	/// IStyleSupportTabs interface.
	/// </summary>
	public interface IStyleSupportTabs
	{
		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="styleTabs">Style for the tabs.</param>
		/// <param name="style">Full style data.</param>
		bool SetStyle(StyleTabs styleTabs, Style style);
	}
}