namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test dialog with different buttons.
	/// </summary>
	public class TestDialogWithDifferentButtons : MonoBehaviour
	{
		/// <summary>
		/// Dialog template.
		/// </summary>
		[SerializeField]
		public Dialog DialogTemplate;

		/// <summary>
		/// Show dialog.
		/// </summary>
		public void ShowDialog()
		{
			var actions = new DialogButton[]
			{
				new DialogButton("Cancel Button", Close, 0),
				new DialogButton("Main Button", Close, 1),
			};

			DialogTemplate.Clone().Show(
				title: "Dialog With Different Buttons",
				message: "Test",
				buttons: actions,
				focusButton: "Close",
				modal: true,
				modalColor: new Color(0, 0, 0, 0.8f),
				onCancel: Close);
		}

		bool Close(int buttonIndex)
		{
			Debug.Log("clicked: " + buttonIndex);
			return true;
		}
	}
}