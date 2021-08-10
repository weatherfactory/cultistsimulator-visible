namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using UIWidgets.l10n;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for the Dialogs.
	/// </summary>
	/// <typeparam name="TDialog">Dialog type.</typeparam>
	public abstract class DialogCustom<TDialog> : MonoBehaviour, ITemplatable, IStylable, IUpgradeable, IHideable
		where TDialog : DialogCustom<TDialog>, IHideable
	{
		/// <summary>
		/// Button instance.
		/// </summary>
		public class ButtonInstance : DialogButtonCustom<TDialog>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ButtonInstance"/> class.
			/// </summary>
			/// <param name="owner">Owner.</param>
			/// <param name="index">Index.</param>
			/// <param name="info">Info.</param>
			/// <param name="template">Template.</param>
			public ButtonInstance(TDialog owner, int index, DialogButton info, Button template)
				: base(owner, index, info, template)
			{
			}
		}

		/// <summary>
		/// Class for the buttons instances.
		/// </summary>
		protected class ButtonsPool : DialogButtonsPoolCustom<TDialog, ButtonInstance>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ButtonsPool"/> class.
			/// </summary>
			/// <param name="owner">Dialog.</param>
			/// <param name="templates">Templates.</param>
			/// <param name="active">List for the active buttons.</param>
			/// <param name="cache">List for the cached buttons.</param>
			public ButtonsPool(TDialog owner, ReadOnlyCollection<Button> templates, List<ButtonInstance> active, List<List<ButtonInstance>> cache)
				: base(owner, templates, active, cache)
			{
			}

			/// <summary>
			/// Create the button.
			/// </summary>
			/// <param name="buttonIndex">Index of the button.</param>
			/// <param name="info">Button info.</param>
			/// <returns>Button.</returns>
			protected override ButtonInstance CreateButtonInstance(int buttonIndex, DialogButton info)
			{
				return new ButtonInstance(Owner, buttonIndex, info, Templates[info.TemplateIndex]);
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
		/// Current buttons.
		/// </summary>
		public ReadOnlyCollection<ButtonInstance> CurrentButtons
		{
			get
			{
				return ButtonsActive.AsReadOnly();
			}
		}

		/// <summary>
		/// The cached buttons.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<List<ButtonInstance>> ButtonsCached = new List<List<ButtonInstance>>();

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
				if (IsTemplate && (buttonsTemplates.Count > value.Count))
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

		string dialogTemplateName;

		/// <summary>
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name of the template.</value>
		public string TemplateName
		{
			get
			{
				if (dialogTemplateName == null)
				{
					Templates.FindTemplates();
				}

				return dialogTemplateName;
			}

			set
			{
				dialogTemplateName = value;
			}
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

		[SerializeField]
		Button closeButton;

		/// <summary>
		/// Close button.
		/// </summary>
		public Button CloseButton
		{
			get
			{
				return closeButton;
			}

			set
			{
				if (isInited && (closeButton != null))
				{
					closeButton.onClick.RemoveListener(Cancel);
				}

				closeButton = value;

				if (isInited && (closeButton != null))
				{
					closeButton.onClick.AddListener(Cancel);
				}
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
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

		bool isInited;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			if (closeButton != null)
			{
				closeButton.onClick.AddListener(Cancel);
			}

			isInited = true;

			Localization.OnLocaleChanged += LocaleChanged;
		}

		/// <summary>
		/// Locale changed.
		/// </summary>
		protected virtual void LocaleChanged()
		{
			Buttons.UpdateButtonsName();
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
			Localization.OnLocaleChanged -= LocaleChanged;

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
		public Action OnClose;

		/// <summary>
		/// Callback on dialog cancel.
		/// </summary>
		public Func<int, bool> OnCancel;

		/// <summary>
		/// Show dialog.
		/// </summary>
		/// <param name="title">Title. Also can be changed with SetInfo().</param>
		/// <param name="message">Message. Also can be changed with SetInfo().</param>
		/// <param name="buttons">Buttons. Also can be changed with SetButtons().</param>
		/// <param name="focusButton">Set focus on button with specified name. Also can be changed with SetButtons() or FocusButton().</param>
		/// <param name="position">Position. Also can be changed with SetPosition().</param>
		/// <param name="icon">Icon. Also can be changed with SetInfo(...).</param>
		/// <param name="modal">If set to <c>true</c> modal. Also can be changed with SetModal().</param>
		/// <param name="modalSprite">Modal sprite. Also can be changed with SetModal().</param>
		/// <param name="modalColor">Modal color. Also can be changed with SetModal().</param>
		/// <param name="canvas">Canvas. Also can be changed with SetCanvas().</param>
		/// <param name="content">Content. Also can be changed with SetContent().</param>
		/// <param name="onClose">On close callback. Also can be changed with OnClose field.</param>
		/// <param name="onCancel">On cancel callback. Also can be changed with OnCancel field.</param>
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

			OnClose = onClose;
			OnCancel = onCancel;
			SetInfo(title, null, message, null, icon);
			SetContent(content);

			SetCanvas(canvas);

			SetModal(modal, modalSprite, modalColor);

			if (position == null)
			{
				position = new Vector3(0, 0, 0);
			}

			SetPosition(position.Value);

			gameObject.SetActive(true);

			SetButtons(buttons, focusButton);
		}

		/// <summary>
		/// Set modal mode.
		/// Warning: modal block is created at the current root canvas.
		/// </summary>
		/// <param name="modal">If set to <c>true</c> modal.</param>
		/// <param name="modalSprite">Modal sprite.</param>
		/// <param name="modalColor">Modal color.</param>
		public virtual void SetModal(bool modal = false, Sprite modalSprite = null, Color? modalColor = null)
		{
			if (ModalKey != null)
			{
				ModalHelper.Close(ModalKey.Value);
				ModalKey = null;
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
		}

		/// <summary>
		/// Set canvas.
		/// </summary>
		/// <param name="canvas">Canvas.</param>
		public virtual void SetCanvas(Canvas canvas)
		{
			var parent = (canvas != null) ? canvas.transform : Utilities.FindTopmostCanvas(gameObject.transform);
			if (parent != null)
			{
				transform.SetParent(parent, false);
			}

			transform.SetAsLastSibling();
		}

		/// <summary>
		/// Set position.
		/// </summary>
		/// <param name="position">Position.</param>
		public virtual void SetPosition(Vector3 position)
		{
			transform.localPosition = position;
		}

		/// <summary>
		/// Sets the info. Pass null to leave default value.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="icon">Icon.</param>
		public virtual void SetInfo(string title = null, string message = null, Sprite icon = null)
		{
			DialogInfo.SetInfo(title, null, message, null, icon);
		}

		/// <summary>
		/// Sets the info. Pass null to leave default value.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="titleArgs">Title arguments.</param>
		/// <param name="message">Message.</param>
		/// <param name="messageArgs">Message arguments.</param>
		/// <param name="icon">Icon.</param>
		public virtual void SetInfo(string title = null, object[] titleArgs = null, string message = null, object[] messageArgs = null, Sprite icon = null)
		{
			DialogInfo.SetInfo(title, titleArgs, message, messageArgs, icon);
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
				ModalHelper.Close(ModalKey.Value);
			}

			Return();
		}

		/// <summary>
		/// Creates the buttons.
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		/// <param name="focusButton">Focus button.</param>
		public virtual void SetButtons(IList<DialogButton> buttons, string focusButton = null)
		{
			Buttons.Disable();

			if (buttons == null)
			{
				return;
			}

			for (int index = 0; index < buttons.Count; index++)
			{
				var info = buttons[index];
				info.TemplateIndex = GetTemplateIndex(info);
				Buttons.Get(index, info);
			}

			FocusButton(focusButton);
		}

		/// <summary>
		/// Set focus to the specified button.
		/// </summary>
		/// <param name="focusButton">Button label.</param>
		/// <returns>true if button found with specified label; otherwise false.</returns>
		public virtual bool FocusButton(string focusButton)
		{
			return Buttons.Focus(focusButton);
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

		/// <inheritdoc/>
		public bool SetStyle(Style style)
		{
			style.Dialog.Background.ApplyTo(GetComponent<Image>());
			style.Dialog.ContentBackground.ApplyTo(transform.Find("Content"));
			style.Dialog.Delimiter.ApplyTo(transform.Find("Delimiter/Delimiter"));

			if (closeButton != null)
			{
				style.ButtonClose.ApplyTo(closeButton);
			}
			else
			{
				style.ButtonClose.Background.ApplyTo(transform.Find("Header/CloseButton"));
				style.ButtonClose.Text.ApplyTo(transform.Find("Header/CloseButton/Text"));
			}

			if (DialogInfo != null)
			{
				DialogInfo.SetStyle(style.Dialog);
			}

			if (buttonsPool != null)
			{
				buttonsPool.SetStyle(style.Dialog.Button);
			}
			else
			{
				for (int template_index = 0; template_index < ButtonsTemplates.Count; template_index++)
				{
					style.Dialog.Button.ApplyTo(ButtonsTemplates[template_index].gameObject);
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public bool GetStyle(Style style)
		{
			style.Dialog.Background.GetFrom(GetComponent<Image>());
			style.Dialog.ContentBackground.GetFrom(transform.Find("Content"));
			style.Dialog.Delimiter.GetFrom(transform.Find("Delimiter/Delimiter"));

			if (closeButton != null)
			{
				style.ButtonClose.GetFrom(closeButton);
			}
			else
			{
				style.ButtonClose.Background.GetFrom(transform.Find("Header/CloseButton"));
				style.ButtonClose.Text.GetFrom(transform.Find("Header/CloseButton/Text"));
			}

			if (DialogInfo != null)
			{
				DialogInfo.GetStyle(style.Dialog);
			}

			for (int template_index = 0; template_index < ButtonsTemplates.Count; template_index++)
			{
				style.Dialog.Button.GetFrom(ButtonsTemplates[template_index].gameObject);
			}

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
				if (btn != null)
				{
					var info = Utilities.GetOrAddComponent<DialogButtonComponentBase>(btn);
					info.Upgrade();
					if (info.NameAdapter == null)
					{
						Utilities.GetOrAddComponent(Compatibility.GetComponentInChildren<Text>(info, true), ref info.NameAdapter);
					}
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
			Compatibility.Upgrade(this);
		}
#endif
	}
}