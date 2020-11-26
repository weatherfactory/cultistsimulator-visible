namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Style for the combobox.
	/// </summary>
	[Serializable]
	public class StyleCombobox : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

		/// <summary>
		/// Style for the input background if multipleselect disabled.
		/// </summary>
		[SerializeField]
		public StyleImage SingleInputBackground;

		/// <summary>
		/// Style for the default item background if multipleselect disabled.
		/// </summary>
		[SerializeField]
		public StyleImage SingleDefaultItemBackground;

		/// <summary>
		/// Style for the default item text if multipleselect disabled.
		/// </summary>
		[SerializeField]
		public StyleText SingleDefaultItemText;

		/// <summary>
		/// Style for the input background if multipleselect enabled.
		/// </summary>
		[SerializeField]
		public StyleImage MultipleInputBackground;

		/// <summary>
		/// Style for the default item background if multipleselect enabled.
		/// </summary>
		[SerializeField]
		public StyleImage MultipleDefaultItemBackground;

		/// <summary>
		/// Style for the default item text if multipleselect enabled.
		/// </summary>
		[SerializeField]
		public StyleText MultipleDefaultItemText;

		/// <summary>
		/// Style for the input.
		/// </summary>
		[SerializeField]
		public StyleText Input;

		/// <summary>
		/// Style for the placeholder.
		/// </summary>
		[SerializeField]
		public StyleText Placeholder;

		/// <summary>
		/// Style for the button.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("Button")]
		public StyleImage ToggleButton;

		/// <summary>
		/// Style for the "Remove" button background.
		/// </summary>
		[SerializeField]
		public StyleImage RemoveBackground;

		/// <summary>
		/// Style for the "Remove" button text.
		/// </summary>
		[SerializeField]
		public StyleText RemoveText;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();

			SingleInputBackground.SetDefaultValues();
			SingleDefaultItemBackground.SetDefaultValues();
			SingleDefaultItemText.SetDefaultValues();

			MultipleInputBackground.SetDefaultValues();
			MultipleDefaultItemBackground.SetDefaultValues();
			MultipleDefaultItemText.SetDefaultValues();

			Input.SetDefaultValues();
			Placeholder.SetDefaultValues();
			ToggleButton.SetDefaultValues();
			RemoveBackground.SetDefaultValues();
			RemoveText.SetDefaultValues();
		}
#endif
	}
}