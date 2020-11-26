namespace UIWidgets.Styles
{
	/// <summary>
	/// IStylable interface.
	/// </summary>
	public interface IStylable
	{
		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		bool SetStyle(Style style);
	}

	/// <summary>
	/// IStylable interface.
	/// </summary>
	/// <typeparam name="T">Type of component to apply style.</typeparam>
	public interface IStylable<T>
	{
		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="styleTyped">Style for specified type.</param>
		/// <param name="style">Full style data.</param>
		bool SetStyle(T styleTyped, Style style);
	}
}