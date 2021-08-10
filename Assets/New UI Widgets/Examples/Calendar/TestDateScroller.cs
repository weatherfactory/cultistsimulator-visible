namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// Test DateScroller.
	/// </summary>
	public class TestDateScroller : MonoBehaviour
	{
		/// <summary>
		/// DateScroller.
		/// </summary>
		[SerializeField]
		protected UIWidgets.DateBase DateScroller;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			DateScroller.OnDateChanged.AddListener(ProcessDate);

			// change culture
			DateScroller.Culture = new System.Globalization.CultureInfo("en-US");

			// change calendar
			DateScroller.Culture = new System.Globalization.CultureInfo("ja-JP");
			DateScroller.Culture.DateTimeFormat.Calendar = new System.Globalization.JapaneseCalendar();
		}

		void ProcessDate(System.DateTime dt)
		{
			Debug.Log(dt);
		}
	}
}