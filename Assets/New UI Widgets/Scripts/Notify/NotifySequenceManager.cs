namespace UIWidgets
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Notification order sequence.
	/// </summary>
	public enum NotifySequence
	{
		/// <summary>
		/// Display notification right now, without adding to sequence.
		/// </summary>
		None = 0,

		/// <summary>
		/// Add notification to start of sequence.
		/// </summary>
		First = 1,

		/// <summary>
		/// Add notification to end of sequence.
		/// </summary>
		Last = 2,
	}

	/// <summary>
	/// Notification sequence manager.
	/// </summary>
	public class NotifySequenceManager : MonoBehaviour
	{
		static readonly List<NotificationBase> NotificationSequence = new List<NotificationBase>();

		static NotificationBase currentNotification;

#if UNITY_2019_3_OR_NEWER
		/// <summary>
		/// Reload support.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void StaticInit()
		{
			currentNotification = null;
			NotificationSequence.Clear();
		}
#endif

		/// <summary>
		/// Clear notifications sequence.
		/// </summary>
		public void Clear()
		{
			if (currentNotification != null)
			{
				currentNotification.Return();
				currentNotification = null;
			}

			NotificationSequence.ForEach(ReturnNotification);
			NotificationSequence.Clear();
		}

		void ReturnNotification(NotificationBase notification)
		{
			notification.Return();
		}

		/// <summary>
		/// Add the specified notification to sequence.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="type">Type.</param>
		public virtual void Add(NotificationBase notification, NotifySequence type)
		{
			if (type == NotifySequence.Last)
			{
				NotificationSequence.Add(notification);
			}
			else
			{
				NotificationSequence.Insert(0, notification);
			}
		}

		/// <summary>
		/// Display next notification in sequence if possible.
		/// </summary>
		protected virtual void Update()
		{
			if (currentNotification != null)
			{
				return;
			}

			if (NotificationSequence.Count == 0)
			{
				return;
			}

			currentNotification = NotificationSequence[0];
			NotificationSequence.RemoveAt(0);
			currentNotification.Display(NotificationDelay);
		}

		IEnumerator nextDelay;

		void NotificationDelay()
		{
			if (nextDelay != null)
			{
				StopCoroutine(nextDelay);
			}

			if ((NotificationSequence.Count > 0) && (NotificationSequence[0].SequenceDelay > 0))
			{
				nextDelay = NextDelay();
				StartCoroutine(nextDelay);
			}
			else
			{
				currentNotification = null;
			}
		}

		/// <summary>
		/// Wait the SequenceDelay seconds.
		/// </summary>
		/// <returns>IEnumerator.</returns>
		protected virtual IEnumerator NextDelay()
		{
			var notify = NotificationSequence[0];
			yield return UtilitiesTime.Wait(notify.SequenceDelay, notify.UnscaledTime);
			currentNotification = null;
		}
	}
}