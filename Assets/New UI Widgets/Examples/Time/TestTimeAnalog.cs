namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Test TimeAnalog.
	/// </summary>
	public class TestTimeAnalog : MonoBehaviour
	{
		/// <summary>
		/// Time widget.
		/// </summary>
		[SerializeField]
		public TimeBase TimeWidget;

		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		public TextAdapter Text;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected virtual void Start()
		{
			TimeWidget.OnTimeChanged.AddListener(TimeChanged);
			TimeChanged(TimeWidget.Time);
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			TimeWidget.OnTimeChanged.RemoveListener(TimeChanged);
		}

		/// <summary>
		/// Process the time changed event.
		/// </summary>
		/// <param name="time">Time.</param>
		public void TimeChanged(TimeSpan time)
		{
			Text.text = string.Format("{0:D02}:{1:D02}", time.Hours, time.Minutes);
		}
	}
}