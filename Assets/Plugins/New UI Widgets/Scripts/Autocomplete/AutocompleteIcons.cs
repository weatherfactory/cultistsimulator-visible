namespace UIWidgets
{
	/// <summary>
	/// Autocomplete for ListViewIcons.
	/// </summary>
	public class AutocompleteIcons : AutocompleteCustom<ListViewIconsItemDescription, ListViewIconsItemComponent, ListViewIcons>
	{
		/// <summary>
		/// Determines whether the beginning of value matches the Input.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if beginning of value matches the Input; otherwise, false.</returns>
		public override bool Startswith(ListViewIconsItemDescription value)
		{
			if (CaseSensitive)
			{
				return value.Name.StartsWith(Query) || (value.LocalizedName != null && value.LocalizedName.StartsWith(Query));
			}

			return value.Name.ToLower().StartsWith(Query.ToLower()) || (value.LocalizedName != null && value.LocalizedName.ToLower().StartsWith(Query.ToLower()));
		}

		/// <summary>
		/// Returns a value indicating whether Input occurs within specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if the Input occurs within value parameter; otherwise, false.</returns>
		public override bool Contains(ListViewIconsItemDescription value)
		{
			if (CaseSensitive)
			{
				return value.Name.Contains(Query) || (value.LocalizedName != null && value.LocalizedName.Contains(Query));
			}

			return value.Name.ToLower().Contains(Query.ToLower()) || (value.LocalizedName != null && value.LocalizedName.ToLower().Contains(Query.ToLower()));
		}

		/// <summary>
		/// Convert value to string.
		/// </summary>
		/// <returns>The string value.</returns>
		/// <param name="value">Value.</param>
		protected override string GetStringValue(ListViewIconsItemDescription value)
		{
			return value.LocalizedName ?? value.Name;
		}
	}
}