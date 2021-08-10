namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Suspends the coroutine execution for the given amount of seconds.
	/// </summary>
	public class WaitForSecondsCustom : CustomYieldInstruction
	{
		/// <summary>
		/// The given amount of seconds that the yield instruction will wait for.
		/// </summary>
		public float Seconds
		{
			get;
			protected set;
		}

		/// <summary>
		/// Use unscaled time.
		/// </summary>
		public bool UnscaledTime
		{
			get;
			protected set;
		}

		private float endTime = -1f;

		/// <summary>
		/// Indicates if coroutine should be kept suspended.
		/// </summary>
		public override bool keepWaiting
		{
			get
			{
				var current_time = UtilitiesTime.GetTime(UnscaledTime);
				if (endTime < 0)
				{
					endTime = current_time + Seconds;
				}

				var waiting = current_time < endTime;
				if (!waiting)
				{
					endTime = -1f;
				}

				return waiting;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WaitForSecondsCustom"/> class.
		/// Creates a yield instruction to wait for a given number of seconds using unscaled time.
		/// </summary>
		/// <param name="seconds">Seconds to wait.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		public WaitForSecondsCustom(float seconds, bool unscaledTime = false)
		{
			Seconds = seconds;
			UnscaledTime = unscaledTime;
		}

		#if UNITY_2020_1_OR_NEWER
		/// <summary>
		/// Reset.
		/// </summary>
		public override void Reset()
		{
			endTime = -1f;
		}
		#endif
	}
}