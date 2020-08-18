namespace UIWidgets
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Notify sequence.
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
	/// Notify sequence manager.
	/// </summary>
	public class NotifySequenceManager : MonoBehaviour
	{
		static readonly List<Notify> NotifySequence = new List<Notify>();

		static Notify currentNotify;

#if UNITY_2019_3_OR_NEWER
		/// <summary>
		/// Reload support.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void StaticInit()
		{
			currentNotify = null;
			NotifySequence.Clear();
		}
#endif

		/// <summary>
		/// Clear notifications sequence.
		/// </summary>
		public void Clear()
		{
			if (currentNotify != null)
			{
				currentNotify.Return();
				currentNotify = null;
			}

			NotifySequence.ForEach(ReturnNotify);
			NotifySequence.Clear();
		}

		void ReturnNotify(Notify notify)
		{
			notify.Return();
		}

		/// <summary>
		/// Add the specified notification to sequence.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="type">Type.</param>
		public virtual void Add(Notify notification, NotifySequence type)
		{
			if (type == UIWidgets.NotifySequence.Last)
			{
				NotifySequence.Add(notification);
			}
			else
			{
				NotifySequence.Insert(0, notification);
			}
		}

		/// <summary>
		/// Display next notification in sequence if possible.
		/// </summary>
		protected virtual void Update()
		{
			if (currentNotify != null)
			{
				return;
			}

			if (NotifySequence.Count == 0)
			{
				return;
			}

			currentNotify = NotifySequence[0];
			NotifySequence.RemoveAt(0);
			currentNotify.Display(NotifyDelay);
		}

		IEnumerator nextDelay;

		void NotifyDelay()
		{
			if (nextDelay != null)
			{
				StopCoroutine(nextDelay);
			}

			if ((NotifySequence.Count > 0) && (NotifySequence[0].SequenceDelay > 0))
			{
				nextDelay = NextDelay();
				StartCoroutine(nextDelay);
			}
			else
			{
				currentNotify = null;
			}
		}

		IEnumerator NextDelay()
		{
			yield return new WaitForSeconds(NotifySequence[0].SequenceDelay);
			currentNotify = null;
		}
	}
}