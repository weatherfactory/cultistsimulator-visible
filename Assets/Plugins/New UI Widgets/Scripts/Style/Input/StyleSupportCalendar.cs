namespace UIWidgets.Styles
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the calendar.
	/// </summary>
	public class StyleSupportCalendar : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Next month.
		/// </summary>
		[SerializeField]
		public Image NextMonth;

		/// <summary>
		/// Previous month.
		/// </summary>
		[SerializeField]
		public Image PrevMonth;

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			style.Calendar.NextMonth.ApplyTo(NextMonth);
			style.Calendar.PrevMonth.ApplyTo(PrevMonth);

			return true;
		}
		#endregion
	}
}