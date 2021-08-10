namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ChatLineOutgoing component.
	/// </summary>
	public class ChatLineOutgoing : ComponentPool<ChatLineOutgoing>, IChatLineComponent, IUpgradeable
	{
		/// <summary>
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>ChatLineComponent instance.</returns>
		public IChatLineComponent IInstance(Transform parent)
		{
			return Instance(parent);
		}

		/// <summary>
		/// Chat.
		/// </summary>
		[SerializeField]
		public ChatView Chat;

		/// <summary>
		/// Invoke chat event.
		/// </summary>
		public void ChatEventInvoke()
		{
			Chat.MyEvent.Invoke();
			Debug.Log("Chat.MyEvent.Invoke()");
		}

		/// <summary>
		/// Username.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with UserNameAdapter.")]
		public Text UserName;

		/// <summary>
		/// Message.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with MessageAdapter.")]
		public Text Message;

		/// <summary>
		/// Message time.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with TimeAdapter.")]
		public Text Time;

		/// <summary>
		/// Username.
		/// </summary>
		[SerializeField]
		public TextAdapter UserNameAdapter;

		/// <summary>
		/// Message.
		/// </summary>
		[SerializeField]
		public TextAdapter MessageAdapter;

		/// <summary>
		/// Message time.
		/// </summary>
		[SerializeField]
		public TextAdapter TimeAdapter;

		/// <summary>
		/// Image.
		/// </summary>
		[SerializeField]
		public Image Image;

		/// <summary>
		/// AudioPlayer.
		/// </summary>
		[SerializeField]
		public AudioPlayer Audio;

		/// <summary>
		/// Lightbox to display image.
		/// </summary>
		[SerializeField]
		public Lightbox Lightbox;

		/// <summary>
		/// Message data.
		/// </summary>
		protected ChatLine Item;

		/// <summary>
		/// Display ChatLine.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(ChatLine item)
		{
			Item = item;

			UserNameAdapter.text = item.UserName;
			MessageAdapter.text = item.Message;
			TimeAdapter.text = item.Time.ToString("[HH:mm:ss]");

			MessageAdapter.gameObject.SetActive(item.Message != null);

			if (Image != null)
			{
				Image.gameObject.SetActive(item.Image != null);
				Image.sprite = item.Image;
			}

			if (Audio != null)
			{
				Audio.gameObject.SetActive(item.Audio != null);
				Audio.SetAudioClip(item.Audio);
			}
		}

		/// <summary>
		/// Show lightbox with image.
		/// </summary>
		public void ShowLightbox()
		{
			Lightbox.Show(Item.Image);
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(UserName, ref UserNameAdapter);
			Utilities.GetOrAddComponent(Message, ref MessageAdapter);
			Utilities.GetOrAddComponent(Time, ref TimeAdapter);
#pragma warning restore 0612, 0618
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected virtual void OnValidate()
		{
			Compatibility.Upgrade(this);
		}
#endif
	}
}