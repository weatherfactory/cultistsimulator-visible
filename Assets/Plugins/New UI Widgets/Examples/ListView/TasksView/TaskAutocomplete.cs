namespace UIWidgets.Examples.Tasks
{
	using UIWidgets;

	/// <summary>
	/// Task Autocomplete.
	/// </summary>
	public class TaskAutocomplete : AutocompleteCustom<Task, TaskComponent, TaskView>
	{
		/// <summary>
		/// Check if task name starts with input string.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>true if task name starts with input string; otherwise, false.</returns>
		public override bool Startswith(Task value)
		{
			if (CaseSensitive)
			{
				return value.Name.StartsWith(Query);
			}

			return value.Name.ToLower().StartsWith(Query.ToLower());
		}

		/// <summary>
		/// Check if task name contains input string.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>true if task name contains input string; otherwise, false.</returns>
		public override bool Contains(Task value)
		{
			if (CaseSensitive)
			{
				return value.Name.Contains(Query);
			}

			return value.Name.ToLower().Contains(Query.ToLower());
		}

		/// <summary>
		/// Convert task to string to display in InputField.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>String to display in InputField.</returns>
		protected override string GetStringValue(Task value)
		{
			return value.Name;
		}
	}
}