namespace UIWidgets
{
	using UIWidgets.Styles;
	using UnityEngine;

	/// <summary>
	/// NotifyInfoBase.
	/// </summary>
	public class NotifyInfoBase : MonoBehaviour, IStylable
	{
		/// <summary>
		/// The message.
		/// </summary>
		[SerializeField]
		public TextAdapter MessageAdapter;

		/// <summary>
		/// Sets the info.
		/// </summary>
		/// <param name="message">Message.</param>
		public virtual void SetInfo(string message)
		{
			SaveDefaultValues();

			if ((MessageAdapter != null) && (message != null))
			{
				MessageAdapter.text = message;
			}
		}

		bool defaultSaved;
		string defaultMessage;

		/// <summary>
		/// Save default values.
		/// </summary>
		public virtual void SaveDefaultValues()
		{
			if (defaultSaved)
			{
				return;
			}

			defaultSaved = true;

			if (MessageAdapter != null)
			{
				defaultMessage = MessageAdapter.text;
			}
		}

		/// <summary>
		/// Restore default values.
		/// </summary>
		public virtual void RestoreDefaultValues()
		{
			if (defaultSaved)
			{
				SetInfo(defaultMessage);
			}
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			if (MessageAdapter != null)
			{
				style.Notify.Text.ApplyTo(MessageAdapter.gameObject);
			}

			return true;
		}
		#endregion
	}
}