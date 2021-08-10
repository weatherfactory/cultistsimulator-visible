namespace UIWidgets
{
	using UIWidgets.l10n;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Button instance.
	/// </summary>
	/// <typeparam name="TOwner">Type of the owner.</typeparam>
	public class DialogButtonCustom<TOwner>
		where TOwner : MonoBehaviour, IHideable
	{
		/// <summary>
		/// Button.
		/// </summary>
		public Button Button
		{
			get;
			private set;
		}

		/// <summary>
		/// Button template index.
		/// </summary>
		public int TemplateIndex
		{
			get
			{
				return Info.TemplateIndex;
			}
		}

		/// <summary>
		/// Button index.
		/// </summary>
		public int Index
		{
			get;
			protected set;
		}

		/// <summary>
		/// Owner.
		/// </summary>
		protected TOwner Owner;

		/// <summary>
		/// Button info component.
		/// </summary>
		public DialogButtonComponentBase ButtonComponent
		{
			get;
			protected set;
		}

		/// <summary>
		/// Button info.
		/// </summary>
		public DialogButton Info
		{
			get;
			protected set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DialogButtonCustom{TOwner}"/> class.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="index">Button index.</param>
		/// <param name="info">Button info.</param>
		/// <param name="template">Template.</param>
		public DialogButtonCustom(TOwner owner, int index, DialogButton info, Button template)
		{
			Owner = owner;
			Index = index;
			Info = info;

			Button = Compatibility.Instantiate(template);
			Button.transform.SetParent(template.transform.parent, false);
			Button.onClick.AddListener(Click);

			ButtonComponent = Button.GetComponent<DialogButtonComponentBase>();
			UpdateButtonName();
		}

		/// <summary>
		/// Change the button index and info.
		/// </summary>
		/// <param name="index">Button index.</param>
		/// <param name="info">Button info.</param>
		public void Change(int index, DialogButton info)
		{
			Index = index;
			Info = info;

			UpdateButtonName();
		}

		/// <summary>
		/// Replace button with the new template.
		/// </summary>
		/// <param name="template">Template.</param>
		public void Replace(Button template)
		{
			DestroyButton();

			Button = Compatibility.Instantiate(template);
			Button.transform.SetParent(template.transform.parent, false);
			ButtonComponent = Button.GetComponent<DialogButtonComponentBase>();

			UpdateButtonName();

			SetActive(true);
		}

		/// <summary>
		/// Update button name.
		/// </summary>
		public virtual void UpdateButtonName()
		{
			if (ButtonComponent != null)
			{
				ButtonComponent.SetButtonName(Localization.GetTranslation(Info.Label));
			}
		}

		/// <summary>
		/// Set button gameObject state.
		/// </summary>
		/// <param name="active">State.</param>
		public void SetActive(bool active)
		{
			Button.gameObject.SetActive(active);

			if (active)
			{
				Button.transform.SetAsLastSibling();
			}
		}

		/// <summary>
		/// Process click event.
		/// </summary>
		protected void Click()
		{
			if (Info.Action(Index))
			{
				Owner.Hide();
			}
		}

		/// <summary>
		/// Destroy button gameobject.
		/// </summary>
		public void Destroy()
		{
			DestroyButton();
			Owner = null;
		}

		void DestroyButton()
		{
			if (Button != null)
			{
				Button.onClick.RemoveListener(Click);
				UnityEngine.Object.Destroy(Button);
			}
		}
	}
}