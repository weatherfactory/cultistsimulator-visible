namespace UIWidgets.Examples
{
	using System.Collections;
	using System.Collections.Generic;
	using EasyLayoutNS;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// Test Notify.
	/// </summary>
	public class TestNotify : MonoBehaviour
	{
		/// <summary>
		/// Notification template.
		/// Gameobject in Hierarchy window, parent gameobject should have Layout component (recommended EasyLayout)
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("notifyPrefab")]
		protected Notify NotifyTemplate;

		/// <summary>
		/// Show Notification.
		/// </summary>
		public void ShowNotify()
		{
			NotifyTemplate.Clone().Show("Achievement unlocked. Hide after 3 seconds.", customHideDelay: 3f);
		}

		/// <summary>
		/// Notification with horizontal collapse animation.
		/// </summary>
		public void HorizontalCollapse()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationCollapseHorizontal, slideUpOnHide: false);
		}

		/// <summary>
		/// Notification with horizontal rotate animation.
		/// </summary>
		public void HorizontalRotate()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationRotateHorizontal, slideUpOnHide: false);
		}

		/// <summary>
		/// Notification with slide animation to left.
		/// </summary>
		public void SlideLeft()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationSlideLeft);
		}

		/// <summary>
		/// Notification with slide animation to right.
		/// </summary>
		public void SlideRight()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationSlideRight);
		}

		/// <summary>
		/// Notification with slide animation to left.
		/// </summary>
		public void SlideLeftCustom()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: x => Notify.AnimationSlideBase(x, true, -1f, 200f, true));
		}

		/// <summary>
		/// Notification with slide animation to right.
		/// </summary>
		public void SlideRightCustom()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: x => Notify.AnimationSlideBase(x, true, +1f, 200f, true));
		}

		/// <summary>
		/// Notification with slide animation to up.
		/// </summary>
		public void SlideUp()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationSlideUp);
		}

		/// <summary>
		/// Notification with slide animation to down.
		/// </summary>
		public void SlideDown()
		{
			NotifyTemplate.Clone().Show("Notification message.", customHideDelay: 3f, hideAnimation: Notify.AnimationSlideDown);
		}
	}
}