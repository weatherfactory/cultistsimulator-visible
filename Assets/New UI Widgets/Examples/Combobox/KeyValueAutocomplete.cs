namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;

	/// <summary>
	/// KeyValueAutocomplete.
	/// </summary>
	public class KeyValueAutocomplete : AutocompleteCustom<KeyValuePair<string, string>, KeyValueListViewItem, KeyValueListView>
	{
		/// <summary>
		/// Determines whether the beginnig of value matches the Input.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if beginnig of value matches the Input; otherwise, false.</returns>
		public override bool Startswith(KeyValuePair<string, string> value)
		{
			var str = value.Key + ";" + value.Value;
			if (CaseSensitive)
			{
				return str.StartsWith(Query);
			}

			return str.ToLower().StartsWith(Query.ToLower());
		}

		/// <summary>
		/// Returns a value indicating whether Input occurs within specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if the Input occurs within value parameter; otherwise, false.</returns>
		public override bool Contains(KeyValuePair<string, string> value)
		{
			var str = value.Key + ";" + value.Value;
			if (CaseSensitive)
			{
				return str.Contains(Query);
			}

			return str.ToLower().Contains(Query.ToLower());
		}

		/// <summary>
		/// Convert value to string.
		/// </summary>
		/// <returns>The string value.</returns>
		/// <param name="value">Value.</param>
		protected override string GetStringValue(KeyValuePair<string, string> value)
		{
			return value.Key + ";" + value.Value;
		}
	}
}