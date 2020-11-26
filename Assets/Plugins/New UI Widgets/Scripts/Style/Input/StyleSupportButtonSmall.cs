namespace UIWidgets.Styles
{
	/// <summary>
	/// Style support for the small button.
	/// </summary>
	public class StyleSupportButtonSmall : StyleSupportButton, IStylable
	{
		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public override bool SetStyle(Style style)
		{
			style.ButtonSmall.ApplyTo(this);

			return true;
		}
		#endregion
	}
}