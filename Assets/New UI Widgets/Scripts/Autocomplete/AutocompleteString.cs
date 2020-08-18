namespace UIWidgets
{
	/// <summary>
	/// Autocomplete for ListViewString.
	/// </summary>
	public class AutocompleteString : AutocompleteCustom<string, ListViewStringItemComponent, ListViewString>
	{
		/// <summary>
		/// Determines whether the beginning of value matches the Input.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if beginning of value matches the Input; otherwise, false.</returns>
		public override bool Startswith(string value)
		{
			if (CaseSensitive)
			{
				return value.StartsWith(Query);
			}

			return value.ToLower().StartsWith(Query.ToLower());
		}

		/// <summary>
		/// Returns a value indicating whether Input occurs within specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if the Input occurs within value parameter; otherwise, false.</returns>
		public override bool Contains(string value)
		{
			if (CaseSensitive)
			{
				return value.Contains(Query);
			}

			return value.ToLower().Contains(Query.ToLower());
		}

		/// <summary>
		/// Convert value to string.
		/// </summary>
		/// <returns>The string value.</returns>
		/// <param name="value">Value.</param>
		protected override string GetStringValue(string value)
		{
			return value;
		}
	}
}