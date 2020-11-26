namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for the Dialogs.
	/// </summary>
	/// <typeparam name="TDialog">Dialog type.</typeparam>
	public class DialogCustom<TDialog> : MonoBehaviour, ITemplatable, IStylable, IUpgradeable
		where TDialog : DialogCustom<TDialog>
	{
		/// <summary>
		/// Button instance.
		/// </summary>
		protected class ButtonInstance
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
			protected int Index;

			/// <summary>
			/// Owner.
			/// </summary>
			protected TDialog Owner;

			/// <summary>
			/// Button info component.
			/// </summary>
			protected DialogButtonComponentBase ButtonComponent;

			/// <summary>
			/// Button info.
			/// </summary>
			protected DialogButton Info;

			/// <summary>
			/// Initializes a new instance of the <see cref="ButtonInstance"/> class.
			/// </summary>
			/// <param name="owner">Owner.</param>
			/// <param name="index">Button index.</param>
			/// <param name="info">Button info.</param>
			/// <param name="template">Template.</param>
			public ButtonInstance(TDialog owner, int index, DialogButton info, Button template)
			{
				Owner = owner;
				Index = index;
				Info = info;

				Button = Compatibility.Instantiate(template);
				Button.transform.SetParent(template.transform.parent, false);
				Button.onClick.AddListener(Click);

				ButtonComponent = Button.GetComponent<DialogButtonComponentBase>();
				if (ButtonComponent != null)
				{
					ButtonComponent.SetButtonName(Info.Label);
				}
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

				if (ButtonComponent != null)
				{
					ButtonComponent.SetButtonName(Info.Label);
				}
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

				if (ButtonComponent != null)
				{
					ButtonComponent.SetButtonName(Info.Label);
				}

				SetActive(true);
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

		/// <summary>
		/// Class for the buttons instances.
		/// </summary>
		protected class ButtonsPool
		{
			/// <summary>
			/// Owner.
			/// </summary>
			protected TDialog Owner;

			/// <summary>
			/// Buttons templates.
			/// </summary>
			protected ReadOnlyCollection<Button> Templates;

			/// <summary>
			/// Active buttons.
			/// </summary>
			protected List<ButtonInstance> Active;

			/// <summary>
			/// Cached buttons.
			/// </summary>
			protected List<List<ButtonInstance>> Cache;

			/// <summary>
			/// Count.
			/// </summary>
			public int Count
			{
				get
				{
					return Templates.Count;
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ButtonsPool"/> class.
			/// </summary>
			/// <param name="owner">Dialog.</param>
			/// <param name="templates">Templates.</param>
			/// <param name="active">List for the active buttons.</param>
			/// <param name="cache">List for the cached buttons.</param>
			public ButtonsPool(TDialog owner, ReadOnlyCollection<Button> templates, List<ButtonInstance> active, List<List<ButtonInstance>> cache)
			{
				Owner = owner;
				Active = active;
				Cache = cache;

				SetTemplates(templates);
			}

			/// <summary>
			/// Set templates.
			/// </summary>
			/// <param name="templates">Templates.</param>
			public void SetTemplates(ReadOnlyCollection<Button> templates)
			{
				Templates = templates;

				for (int i = 0; i < Templates.Count; i++)
				{
					Templates[i].gameObject.SetActive(false);
				}

				EnsureListSize(Cache, Templates.Count);
			}

			/// <summary>
			/// Ensure list size.
			/// </summary>
			/// <typeparam name="T">Type of the list data.</typeparam>
			/// <param name="list">List.</param>
			/// <param name="size">Required size.</param>
			protected static void EnsureListSize<T>(List<List<T>> list, int size)
			{
				for (int i = list.Count; i < size; i++)
				{
					list.Add(new List<T>());
				}

				for (int i = size; i > list.Count; i--)
				{
					list.RemoveAt(i - 1);
				}
			}

			/// <summary>
			/// Get the button.
			/// </summary>
			/// <param name="buttonIndex">Index of the button.</param>
			/// <param name="info">Button info.</param>
			/// <returns>Button.</returns>
			public ButtonInstance Get(int buttonIndex, DialogButton info)
			{
				ButtonInstance instance;
				if (Cache[info.TemplateIndex].Count > 0)
				{
					instance = Cache[info.TemplateIndex].Pop();
					instance.Change(buttonIndex, info);
				}
				else
				{
					instance = new ButtonInstance(Owner, buttonIndex, info, Templates[info.TemplateIndex]);
				}

				Active.Add(instance);

				instance.SetActive(true);

				return instance;
			}

			/// <summary>
			/// Replace buttons templates.
			/// </summary>
			/// <param name="templates">Templates.</param>
			public void Replace(ReadOnlyCollection<Button> templates)
			{
				ClearCache();

				SetTemplates(templates);

				for (int button_index = 0; button_index < Active.Count; button_index++)
				{
					var button = Active[button_index];
					button.Replace(Templates[button.TemplateIndex]);
				}
			}

			/// <summary>
			/// Disable.
			/// </summary>
			public void Disable()
			{
				for (int button_index = 0; button_index < Active.Count; button_index++)
				{
					var button = Active[button_index];
					button.SetActive(false);

					Cache[button.TemplateIndex].Add(button);
				}

				Active.Clear();
			}

			/// <summary>
			/// Clear cache.
			/// </summary>
			protected void ClearCache()
			{
				for (int template_index = 0; template_index < Cache.Count; template_index++)
				{
					for (int i = 0; i < Cache[template_index].Count; i++)
					{
						Cache[template_index][i].Destroy();
					}

					Cache[template_index].Clear();
				}
			}

			/// <summary>
			/// Execute action for each button.
			/// </summary>
			/// <param name="action">Action.</param>
			public void ForEach(Action<Button> action)
			{
				for (int template_index = 0; template_index < Templates.Count; template_index++)
				{
					action(Templates[template_index]);

					for (int button_index = 0; button_index < Cache[template_index].Count; button_index++)
					{
						action(Cache[template_index][button_index].Button);
					}
				}

				for (int button_index = 0; button_index < Active.Count; button_index++)
				{
					action(Active[button_index].Button);
				}
			}

			/// <summary>
			/// Set style.
			/// </summary>
			/// <param name="styleButton">Button style.</param>
			public void SetStyle(StyleButton styleButton)
			{
				for (int template_index = 0; template_index < Templates.Count; template_index++)
				{
					styleButton.ApplyTo(Templates[template_index].gameObject);

					for (int button_index = 0; button_index < Cache[template_index].Count; button_index++)
					{
						styleButton.ApplyTo(Cache[template_index][button_index].Button.gameObject);
					}
				}

				for (int button_index = 0; button_index < Active.Count; button_index++)
				{
					styleButton.ApplyTo(Active[button_index].Button.gameObject);
				}
			}
		}

		ButtonsPool buttonsPool;

		/// <summary>
		/// Buttons pool.
		/// </summary>
		protected ButtonsPool Buttons
		{
			get
			{
				if (buttonsPool == null)
				{
					buttonsPool = new ButtonsPool(this as TDialog, ButtonsTemplates, ButtonsActive, ButtonsCached);
				}

				return buttonsPool;
			}
		}

		/// <summary>
		/// The buttons in use.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<ButtonInstance> ButtonsActive = new List<ButtonInstance>();

		/// <summary>
		/// The cached buttons.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<List<ButtonInstance>> ButtonsCached = new List<List<ButtonInstance>>();

		/// <summary>
		/// The buttons labels.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<List<string>> ButtonsLabels = new List<List<string>>();

		/// <summary>
		/// The buttons actions.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<List<UnityAction>> ButtonsActions = new List<List<UnityAction>>();

#pragma warning disable 0649
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with buttonsTemplates")]
		Button defaultButton;
#pragma warning restore 0649

		/// <summary>
		/// Gets or sets the default button.
		/// </summary>
		/// <value>The default button.</value>
		[Obsolete("Replaced with ButtonsTemplates")]
		public Button DefaultButton
		{
			get
			{
				Upgrade();
				return defaultButton;
			}

			set
			{
				Upgrade();
				var buttons = new List<Button>(ButtonsTemplates)
				{
					value,
				};
				ButtonsTemplates = buttons.AsReadOnly();
			}
		}

		[SerializeField]
		List<Button> buttonsTemplates = new List<Button>();

		/// <summary>
		/// Gets or sets the default buttons.
		/// </summary>
		/// <value>The default buttons.</value>
		public ReadOnlyCollection<Button> ButtonsTemplates
		{
			get
			{
				Upgrade();

				return buttonsTemplates.AsReadOnly();
			}

			set
			{
				if (isTemplate && (buttonsTemplates.Count > value.Count))
				{
					throw new ArgumentOutOfRangeException("value", string.Format("Buttons count cannot be decreased. Current is {0}; New is {1}", buttonsTemplates.Count, value.Count));
				}

				Buttons.Replace(value);

				buttonsTemplates.Clear();
				buttonsTemplates.AddRange(value);

				if (buttonsTemplates.Count > 0)
				{
					Upgrade();
				}
			}
		}

		/// <summary>
		/// Content root.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with DialogInfo component.")]
		public RectTransform ContentRoot;

		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with DialogInfo component.")]
		Text titleText;

		/// <summary>
		/// Gets or sets the text component.
		/// </summary>
		/// <value>The text.</value>
		[Obsolete("Replaced with DialogInfo component.")]
		public Text TitleText
		{
			get
			{
				return titleText;
			}

			set
			{
				titleText = value;
			}
		}

		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with DialogInfo component.")]
		Text contentText;

		/// <summary>
		/// Gets or sets the text component.
		/// </summary>
		/// <value>The text.</value>
		[Obsolete("Replaced with DialogInfo component.")]
		public Text ContentText
		{
			get
			{
				return contentText;
			}

			set
			{
				contentText = value;
			}
		}

		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with DialogInfo component.")]
		Image dialogIcon;

		/// <summary>
		/// Gets or sets the icon component.
		/// </summary>
		/// <value>The icon.</value>
		[Obsolete("Replaced with DialogInfo component.")]
		public Image Icon
		{
			get
			{
				return dialogIcon;
			}

			set
			{
				dialogIcon = value;
			}
		}

		[SerializeField]
		DialogInfoBase dialogInfo;

		/// <summary>
		/// Gets the dialog info.
		/// </summary>
		/// <value>The dialog info.</value>
		public DialogInfoBase DialogInfo
		{
			get
			{
				if (dialogInfo == null)
				{
					dialogInfo = GetComponent<DialogInfoBase>();
				}

				return dialogInfo;
			}
		}

		bool isTemplate = true;

		/// <summary>
		/// Gets a value indicating whether this instance is template.
		/// </summary>
		/// <value><c>true</c> if this instance is template; otherwise, <c>false</c>.</value>
		public bool IsTemplate
		{
			get
			{
				return isTemplate;
			}

			set
			{
				isTemplate = value;
			}
		}

		/// <summary>
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name of the template.</value>
		public string TemplateName
		{
			get;
			set;
		}

		static Templates<TDialog> templates;

		/// <summary>
		/// Dialog templates.
		/// </summary>
		public static Templates<TDialog> Templates
		{
			get
			{
				if (templates == null)
				{
					templates = new Templates<TDialog>();
				}

				return templates;
			}

			set
			{
				templates = value;
			}
		}

		/// <summary>
		/// Opened dialogs.
		/// </summary>
		protected static HashSet<TDialog> openedDialogs = new HashSet<TDialog>();

		/// <summary>
		/// List of the opened dialogs.
		/// </summary>
		protected static List<TDialog> OpenedDialogsList = new List<TDialog>();

		/// <summary>
		/// Opened dialogs.
		/// </summary>
		public static ReadOnlyCollection<TDialog> OpenedDialogs
		{
			get
			{
				OpenedDialogsList.Clear();
				OpenedDialogsList.AddRange(openedDialogs);

				return OpenedDialogsList.AsReadOnly();
			}
		}

		/// <summary>
		/// Count of the opened dialogs.
		/// </summary>
		public static int Opened
		{
			get
			{
				return openedDialogs.Count;
			}
		}

		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		protected virtual void Awake()
		{
			if (IsTemplate)
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Process enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			openedDialogs.Add(this as TDialog);
		}

		/// <summary>
		/// Process disable event.
		/// </summary>
		protected virtual void OnDisable()
		{
			openedDialogs.Remove(this as TDialog);
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
			Buttons.Disable();

			if (!IsTemplate)
			{
				templates = null;
				return;
			}

			// if FindTemplates never called than TemplateName == null
			if (TemplateName != null)
			{
				Templates.Delete(TemplateName);
			}
		}

		/// <summary>
		/// Return dialog instance by the specified template name.
		/// </summary>
		/// <param name="templateName">Template name.</param>
		/// <returns>New Dialog instance.</returns>
		[Obsolete("Use Clone(templateName) instead.")]
		public static TDialog Template(string templateName)
		{
			return Clone(templateName);
		}

		/// <summary>
		/// Return dialog instance using current instance as template.
		/// </summary>
		/// <returns>New Dialog instance.</returns>
		[Obsolete("Use Clone() instead.")]
		public TDialog Template()
		{
			return Clone();
		}

		/// <summary>
		/// Return dialog instance by the specified template name.
		/// </summary>
		/// <param name="templateName">Template name.</param>
		/// <returns>New Dialog instance.</returns>
		public static TDialog Clone(string templateName)
		{
			return Templates.Instance(templateName);
		}

		/// <summary>
		/// Return dialog instance using current instance as template.
		/// </summary>
		/// <returns>New Dialog instance.</returns>
		public TDialog Clone()
		{
			var dialog = this as TDialog;
			if ((TemplateName != null) && Templates.Exists(TemplateName))
			{
				// do nothing
			}
			else if (!Templates.Exists(gameObject.name))
			{
				Templates.Add(gameObject.name, dialog);
			}
			else if (Templates.Get(gameObject.name) != dialog)
			{
				Templates.Add(gameObject.name, dialog);
			}

			var id = gameObject.GetInstanceID().ToString();
			if (!Templates.Exists(id))
			{
				Templates.Add(id, dialog);
			}
			else if (Templates.Get(id) != dialog)
			{
				Templates.Add(id, dialog);
			}

			return Templates.Instance(id);
		}

		/// <summary>
		/// The modal key.
		/// </summary>
		protected int? ModalKey;

		/// <summary>
		/// Callback on dialog close.
		/// </summary>
		protected Action OnClose;

		/// <summary>
		/// Callback on dialog cancel.
		/// </summary>
		protected Func<int, bool> OnCancel;

		/// <summary>
		/// Show dialog.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="buttons">Buttons.</param>
		/// <param name="focusButton">Set focus on button with specified name.</param>
		/// <param name="position">Position.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="modal">If set to <c>true</c> modal.</param>
		/// <param name="modalSprite">Modal sprite.</param>
		/// <param name="modalColor">Modal color.</param>
		/// <param name="canvas">Canvas.</param>
		/// <param name="content">Content.</param>
		/// <param name="onClose">On close callback.</param>
		/// <param name="onCancel">On cancel callback.</param>
		public virtual void Show(
			string title = null,
			string message = null,
			IList<DialogButton> buttons = null,
			string focusButton = null,
			Vector3? position = null,
			Sprite icon = null,
			bool modal = false,
			Sprite modalSprite = null,
			Color? modalColor = null,
			Canvas canvas = null,
			RectTransform content = null,
			Action onClose = null,
			Func<int, bool> onCancel = null)
		{
			if (IsTemplate)
			{
				Debug.LogWarning("Use the template clone, not the template itself: DialogTemplate.Clone().Show(...), not DialogTemplate.Show(...)");
			}

			if (position == null)
			{
				position = new Vector3(0, 0, 0);
			}

			OnClose = onClose;
			OnCancel = onCancel;
			SetInfo(title, message, icon);
			SetContent(content);

			var parent = (canvas != null) ? canvas.transform : Utilities.FindTopmostCanvas(gameObject.transform);
			if (parent != null)
			{
				transform.SetParent(parent, false);
			}

			if (modal)
			{
				ModalKey = ModalHelper.Open(this, modalSprite, modalColor);
			}
			else
			{
				ModalKey = null;
			}

			transform.SetAsLastSibling();

			transform.localPosition = (Vector3)position;
			gameObject.SetActive(true);

			CreateButtons(buttons, focusButton);
		}

		/// <summary>
		/// Sets the info.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="icon">Icon.</param>
		public virtual void SetInfo(string title = null, string message = null, Sprite icon = null)
		{
			DialogInfo.SetInfo(title, message, icon);
		}

		/// <summary>
		/// Sets the content.
		/// </summary>
		/// <param name="content">Content.</param>
		public virtual void SetContent(RectTransform content)
		{
			if (content == null)
			{
				return;
			}

			DialogInfo.SetContent(content);
		}

		/// <summary>
		/// Cancel dialog.
		/// </summary>
		public virtual void Cancel()
		{
			if (OnCancel != null)
			{
				if (!OnCancel(-1))
				{
					return;
				}
			}

			Hide();
		}

		/// <summary>
		/// Close dialog.
		/// </summary>
		public virtual void Hide()
		{
			if (OnClose != null)
			{
				OnClose();
			}

			if (ModalKey != null)
			{
				ModalHelper.Close((int)ModalKey);
			}

			Return();
		}

		/// <summary>
		/// Creates the buttons.
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		/// <param name="focusButton">Focus button.</param>
		protected virtual void CreateButtons(IList<DialogButton> buttons, string focusButton)
		{
			if (buttons == null)
			{
				return;
			}

			for (int index = 0; index < buttons.Count; index++)
			{
				var info = buttons[index];
				info.TemplateIndex = GetTemplateIndex(info);
				var btn = Buttons.Get(index, info);

				if (info.Label == focusButton)
				{
					btn.Button.Select();
				}
			}
		}

		/// <summary>
		/// Get button template index.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <returns>Template index,</returns>
		protected int GetTemplateIndex(DialogButton button)
		{
			var template = button.TemplateIndex;
			if (template < 0)
			{
				Debug.LogWarning(string.Format("Negative button index not supported. Button: {0}. Index: {1}.", button.Label, template));
				template = 0;
			}

			if (template >= Buttons.Count)
			{
				Debug.LogWarning(string.Format(
					"Not found button template with index {0} for the button: {1}. Available indices: 0..{2}",
					template,
					button.Label,
					Buttons.Count - 1));
				template = 0;
			}

			return template;
		}

		/// <summary>
		/// Return this instance to cache.
		/// </summary>
		protected virtual void Return()
		{
			Templates.ToCache(this as TDialog);

			Buttons.Disable();
			ResetParameters();
		}

		/// <summary>
		/// Resets the parameters.
		/// </summary>
		protected virtual void ResetParameters()
		{
			OnClose = null;
			OnCancel = null;

			DialogInfo.RestoreDefaultValues();
		}

		/// <summary>
		/// Default function to close dialog.
		/// </summary>
		/// <returns>true if dialog can be closed; otherwise false.</returns>
		public static bool Close()
		{
			return true;
		}

		/// <summary>
		/// Default function to close dialog.
		/// </summary>
		/// <param name="index">Button index.</param>
		/// <returns>true if dialog can be closed; otherwise false.</returns>
		public static bool AlwaysClose(int index)
		{
			return true;
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public bool SetStyle(Style style)
		{
			style.Dialog.Background.ApplyTo(GetComponent<Image>());
			style.Dialog.ContentBackground.ApplyTo(transform.Find("Content"));
			style.Dialog.Delimiter.ApplyTo(transform.Find("Delimiter/Delimiter"));
			style.ButtonClose.Background.ApplyTo(transform.Find("Header/CloseButton"));

			if (DialogInfo != null)
			{
				DialogInfo.SetStyle(style.Dialog);
			}

			Buttons.SetStyle(style.Dialog.Button);

			return true;
		}
		#endregion

		/// <summary>
		/// Upgrade fields data to the latest version.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0618
			if ((buttonsTemplates.Count == 0) && (defaultButton != null))
			{
				buttonsTemplates.Add(defaultButton);
			}

			foreach (var btn in buttonsTemplates)
			{
				var info = Utilities.GetOrAddComponent<DialogButtonComponentBase>(btn);
				info.Upgrade();
				if (info.NameAdapter == null)
				{
					Utilities.GetOrAddComponent(Compatibility.GetComponentInChildren<Text>(info, true), ref info.NameAdapter);
				}
			}

			if (ContentRoot == null)
			{
				ContentRoot = transform.Find("Content") as RectTransform;
			}

			if (dialogInfo == null)
			{
				dialogInfo = Utilities.GetOrAddComponent<DialogInfoBase>(this);
				Utilities.GetOrAddComponent(titleText, ref dialogInfo.TitleAdapter);
				Utilities.GetOrAddComponent(contentText, ref dialogInfo.MessageAdapter);
				dialogInfo.Icon = Icon;
			}

			if (dialogInfo.ContentRoot == null)
			{
				dialogInfo.ContentRoot = ContentRoot;
			}
#pragma warning restore 0618
		}

#if UNITY_EDITOR
		/// <summary>
		/// Update layout when parameters changed.
		/// </summary>
		protected virtual void OnValidate()
		{
			if (!Compatibility.IsPrefab(this))
			{
				Upgrade();
			}
		}
#endif
	}
}