namespace UIWidgets
{
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// DialogInfoBase.
	/// </summary>
	public class DialogInfoBase : MonoBehaviour
	{
		/// <summary>
		/// The title.
		/// </summary>
		[SerializeField]
		public TextAdapter TitleAdapter;

		/// <summary>
		/// The message.
		/// </summary>
		[SerializeField]
		public TextAdapter MessageAdapter;

		/// <summary>
		/// The icon.
		/// </summary>
		[SerializeField]
		public Image Icon;

		/// <summary>
		/// Content.
		/// </summary>
		[SerializeField]
		public RectTransform ContentRoot;

		/// <summary>
		/// Sets the info.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="icon">Icon.</param>
		public virtual void SetInfo(string title, string message, Sprite icon)
		{
			SaveDefaultValues();

			if ((title != null) && (TitleAdapter != null))
			{
				TitleAdapter.text = title;
			}

			if ((message != null) && (MessageAdapter != null))
			{
				MessageAdapter.text = message;
			}

			if ((icon != null) && (Icon != null))
			{
				Icon.sprite = icon;
			}
		}

		bool defaultSaved;
		string defaultTitle;
		string defaultMessage;
		Sprite defaultIcon;

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

			if (TitleAdapter != null)
			{
				defaultTitle = TitleAdapter.text;
			}

			if (MessageAdapter != null)
			{
				defaultMessage = MessageAdapter.text;
			}

			if (Icon != null)
			{
				defaultIcon = Icon.sprite;
			}
		}

		/// <summary>
		/// Restore default values.
		/// </summary>
		public virtual void RestoreDefaultValues()
		{
			if (defaultSaved)
			{
				SetInfo(defaultTitle, defaultMessage, defaultIcon);
			}
		}

		/// <summary>
		/// Set content.
		/// </summary>
		/// <param name="content">Content.</param>
		public virtual void SetContent(RectTransform content)
		{
			if (content == null)
			{
				return;
			}

			if (ContentRoot == null)
			{
				Debug.LogWarning("ContentRoot not specified.", this);
				return;
			}

			content.SetParent(ContentRoot, false);
		}

		/// <summary>
		/// Set style.
		/// </summary>
		/// <param name="style">Dialog style.</param>
		public virtual void SetStyle(StyleDialog style)
		{
			if (TitleAdapter != null)
			{
				style.Title.ApplyTo(TitleAdapter.gameObject);
			}

			if (MessageAdapter != null)
			{
				style.ContentText.ApplyTo(MessageAdapter.gameObject);
			}

			style.ContentBackground.ApplyTo(ContentRoot);
		}
	}
}