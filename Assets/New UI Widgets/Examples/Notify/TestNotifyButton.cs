namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test notification with buttons.
	/// </summary>
	public class TestNotifyButton : MonoBehaviour
	{
		/// <summary>
		/// Notification template.
		/// Gameobject in Hierarchy window, parent gameobject should have Layout component (recommended EasyLayout)
		/// </summary>
		[SerializeField]
		protected Notify NotificationTemplate;

		/// <summary>
		/// Show notification.
		/// </summary>
		public void ShowNotify()
		{
			var actions = new DialogButton[]
			{
				new DialogButton("Close", NotificationClose),
				new DialogButton("Log", NotificationClick),
			};

			var instance = NotificationTemplate.Clone();
			instance.Show("Notification with buttons. Hide after 3 seconds.", customHideDelay: 5f);
			instance.SetButtons(actions);
		}

		bool NotificationClose(int index)
		{
			Debug.Log("close notification");
			return true;
		}

		bool NotificationClick(int index)
		{
			Debug.Log("click notification button");
			return false;
		}
	}
}