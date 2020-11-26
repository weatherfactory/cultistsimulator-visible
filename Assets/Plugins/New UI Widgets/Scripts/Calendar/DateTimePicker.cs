namespace UIWidgets
{
	using System;
	using UIWidgets.Styles;
	using UnityEngine;

	/// <summary>
	/// DatePicker.
	/// </summary>
	public class DateTimePicker : Picker<DateTime, DateTimePicker>
	{
		/// <summary>
		/// DateTime widget.
		/// </summary>
		[SerializeField]
		public DateTimeWidget DateTimeWidget;

		/// <summary>
		/// The value.
		/// </summary>
		protected DateTime Value;

		/// <summary>
		/// Prepare picker to open.
		/// </summary>
		/// <param name="defaultValue">Default value.</param>
		public override void BeforeOpen(DateTime defaultValue)
		{
			Value = defaultValue;
			DateTimeWidget.DateTime = defaultValue;

			DateTimeWidget.OnDateTimeChanged.AddListener(DateTimeSelected);
		}

		/// <summary>
		/// Process selected value.
		/// </summary>
		/// <param name="dt">Selected value.</param>
		protected void DateTimeSelected(DateTime dt)
		{
			Value = dt;
		}

		/// <summary>
		/// Pick the selected value.
		/// </summary>
		public void Ok()
		{
			Selected(Value);
		}

		/// <summary>
		/// Prepare picker to close.
		/// </summary>
		public override void BeforeClose()
		{
			DateTimeWidget.OnDateTimeChanged.RemoveListener(DateTimeSelected);
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public override bool SetStyle(Style style)
		{
			base.SetStyle(style);

			DateTimeWidget.Calendar.SetStyle(style);
			DateTimeWidget.Time.SetStyle(style);

			style.Dialog.Button.ApplyTo(transform.Find("Buttons/Cancel"));
			style.Dialog.Button.ApplyTo(transform.Find("Buttons/OK"));

			return true;
		}
		#endregion
	}
}