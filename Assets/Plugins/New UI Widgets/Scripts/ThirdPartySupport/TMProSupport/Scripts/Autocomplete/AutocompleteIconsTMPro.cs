#if UIWIDGETS_TMPRO_SUPPORT && (UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER)
namespace UIWidgets.TMProSupport
{
	/// <summary>
	/// Autocomplete for ListViewIcons.
	/// </summary>
	public class AutocompleteIconsTMPro : AutocompleteCustomTMPro<ListViewIconsItemDescription, ListViewIconsItemComponent, ListViewIcons>
	{
		/// <summary>
		/// Determines whether the beginning of value matches the Query.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if beginning of value matches the Query; otherwise, false.</returns>
		public override bool Startswith(ListViewIconsItemDescription value)
		{
			if (CaseSensitive)
			{
				return value.Name.StartsWith(Query) || (value.LocalizedName != null && value.LocalizedName.StartsWith(Query));
			}

			return value.Name.ToLower().StartsWith(Query.ToLower()) || (value.LocalizedName != null && value.LocalizedName.ToLower().StartsWith(Query.ToLower()));
		}

		/// <summary>
		/// Returns a value indicating whether Query occurs within specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if the Query occurs within value parameter; otherwise, false.</returns>
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
#endif