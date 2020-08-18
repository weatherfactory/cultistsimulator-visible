namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for creating own tabs.
	/// </summary>
	/// <typeparam name="TTab">Type of tab data.</typeparam>
	/// <typeparam name="TButton">Type of tab button.</typeparam>
	public class TabsCustom<TTab, TButton> : MonoBehaviour, IStylable<StyleTabs>
		where TTab : Tab
		where TButton : TabButton
	{
		/// <summary>
		/// The container for tab toggle buttons.
		/// </summary>
		[SerializeField]
		public Transform Container;

		/// <summary>
		/// The default tab button.
		/// </summary>
		[SerializeField]
		public TButton DefaultTabButton;

		/// <summary>
		/// The active tab button.
		/// </summary>
		[SerializeField]
		public TButton ActiveTabButton;

		[SerializeField]
		TTab[] tabObjects = new TTab[] { };

		/// <summary>
		/// Gets or sets the tab objects.
		/// </summary>
		/// <value>The tab objects.</value>
		public TTab[] TabObjects
		{
			get
			{
				return tabObjects;
			}

			set
			{
				tabObjects = value;
				UpdateButtons();
			}
		}

		/// <summary>
		/// The name of the default tab.
		/// </summary>
		[SerializeField]
		[Tooltip("Tab name which will be active by default, if not specified will be opened first Tab.")]
		public string DefaultTabName = string.Empty;

		/// <summary>
		/// If true does not deactivate hidden tabs.
		/// </summary>
		[SerializeField]
		[Tooltip("If true does not deactivate hidden tabs.")]
		public bool KeepTabsActive = false;

		/// <summary>
		/// OnTabSelect event.
		/// </summary>
		[SerializeField]
		public TabSelectEvent OnTabSelect = new TabSelectEvent();

		/// <summary>
		/// Gets or sets the selected tab.
		/// </summary>
		/// <value>The selected tab.</value>
		public TTab SelectedTab
		{
			get;
			protected set;
		}

		/// <summary>
		/// Index of the selected tab.
		/// </summary>
		public int SelectedTabIndex
		{
			get
			{
				return Array.IndexOf(TabObjects, SelectedTab);
			}
		}

		/// <summary>
		/// The default buttons.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TButton> DefaultButtons = new List<TButton>();

		/// <summary>
		/// The active buttons.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TButton> ActiveButtons = new List<TButton>();

		/// <summary>
		/// The callbacks.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<UnityAction> Callbacks = new List<UnityAction>();

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;
			if (Container == null)
			{
				throw new NullReferenceException("Container is null. Set object of type GameObject to Container.");
			}

			if (DefaultTabButton == null)
			{
				throw new NullReferenceException("DefaultTabButton is null. Set object of type GameObject to DefaultTabButton.");
			}

			if (ActiveTabButton == null)
			{
				throw new NullReferenceException("ActiveTabButton is null. Set object of type GameObject to ActiveTabButton.");
			}

			DefaultTabButton.gameObject.SetActive(false);
			ActiveTabButton.gameObject.SetActive(false);

			UpdateButtons();
		}

		/// <summary>
		/// Updates the buttons.
		/// </summary>
		protected virtual void UpdateButtons()
		{
			RemoveCallbacks();

			CreateButtons();

			AddCallbacks();

			if (tabObjects.Length == 0)
			{
				return;
			}

			if (!string.IsNullOrEmpty(DefaultTabName))
			{
				var tab = GetTabByName(DefaultTabName);
				if (tab != null)
				{
					SelectTab(tab);
				}
				else
				{
					Debug.LogWarning(string.Format("Tab with specified DefaultTabName \"{0}\" not found. Opened first Tab.", DefaultTabName), this);
					SelectTab(tabObjects[0]);
				}
			}
			else
			{
				SelectTab(tabObjects[0]);
			}
		}

		TTab GetTabByName(string tabName)
		{
			for (int i = 0; i < tabObjects.Length; i++)
			{
				if (tabObjects[i].Name == tabName)
				{
					return tabObjects[i];
				}
			}

			return null;
		}

		/// <summary>
		/// Add callback.
		/// </summary>
		/// <param name="tab">Tab.</param>
		/// <param name="index">Tab index.</param>
		protected virtual void AddCallback(TTab tab, int index)
		{
#if CSHARP_7_3_OR_NEWER
			void Callback()
#else
			UnityAction Callback = () =>
#endif
			{
				SelectTab(tab);
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			Callbacks.Add(Callback);

			DefaultButtons[index].onClick.AddListener(Callbacks[index]);
		}

		/// <summary>
		/// Add callbacks.
		/// </summary>
		protected void AddCallbacks()
		{
			tabObjects.ForEach(AddCallback);
		}

		/// <summary>
		/// Remove callbacks.
		/// </summary>
		/// <param name="tab">Tab.</param>
		/// <param name="index">Tab index.</param>
		protected virtual void RemoveCallback(TTab tab, int index)
		{
			if ((tab != null) && (index < Callbacks.Count))
			{
				DefaultButtons[index].onClick.RemoveListener(Callbacks[index]);
			}
		}

		/// <summary>
		/// Remove callbacks.
		/// </summary>
		protected void RemoveCallbacks()
		{
			if (Callbacks.Count > 0)
			{
				tabObjects.ForEach(RemoveCallback);
				Callbacks.Clear();
			}
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			RemoveCallbacks();
		}

		/// <summary>
		/// Selects the tab.
		/// </summary>
		/// <param name="tabName">Tab name.</param>
		public void SelectTab(string tabName)
		{
			var tab = GetTabByName(tabName);
			if (tab != null)
			{
				SelectTab(tab);
			}
			else
			{
				Debug.LogWarning(string.Format("Tab with specified name \"{0}\" not found.", tabName), this);
			}
		}

		/// <summary>
		/// Selects the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public void SelectTab(TTab tab)
		{
			var index = Array.IndexOf(tabObjects, tab);
			if (index == -1)
			{
				throw new ArgumentException(string.Format("Tab with name \"{0}\" not found.", tab.Name));
			}

			SelectedTab = tabObjects[index];

			if (KeepTabsActive)
			{
				tabObjects[index].TabObject.transform.SetAsLastSibling();
			}
			else
			{
				tabObjects.ForEach(DeactivateTab);
				tabObjects[index].TabObject.SetActive(true);
			}

			DefaultButtons.ForEach(ActivateButton);
			DefaultButtons[index].gameObject.SetActive(false);

			ActiveButtons.ForEach(DeactivateButton);
			ActiveButtons[index].gameObject.SetActive(true);

			OnTabSelect.Invoke(index);

			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ActiveButtons[index].gameObject);
		}

		void DeactivateTab(TTab tab)
		{
			tab.TabObject.SetActive(false);
		}

		void ActivateButton(TButton button)
		{
			button.gameObject.SetActive(true);
		}

		void DeactivateButton(TButton button)
		{
			button.gameObject.SetActive(false);
		}

		/// <summary>
		/// Enable button interactions.
		/// </summary>
		/// <param name="button">Button.</param>
		protected void EnableInteractable(Button button)
		{
			button.interactable = true;
		}

		/// <summary>
		/// Creates the buttons.
		/// </summary>
		void CreateButtons()
		{
			DefaultButtons.ForEach(EnableInteractable);
			ActiveButtons.ForEach(EnableInteractable);
			if (tabObjects.Length > DefaultButtons.Count)
			{
				for (var i = DefaultButtons.Count; i < tabObjects.Length; i++)
				{
					var defaultButton = Compatibility.Instantiate(DefaultTabButton);
					defaultButton.transform.SetParent(Container, false);
					DefaultButtons.Add(defaultButton);

					var activeButton = Compatibility.Instantiate(ActiveTabButton);
					activeButton.transform.SetParent(Container, false);
					ActiveButtons.Add(activeButton);
				}
			}

			// del existing ui elements if necessary
			if (tabObjects.Length < DefaultButtons.Count)
			{
				for (var i = DefaultButtons.Count - 1; i > tabObjects.Length - 1; i--)
				{
					Destroy(DefaultButtons[i].gameObject);
					Destroy(ActiveButtons[i].gameObject);

					DefaultButtons.RemoveAt(i);
					ActiveButtons.RemoveAt(i);
				}
			}

			DefaultButtons.ForEach(SetButtonData);
			ActiveButtons.ForEach(SetButtonData);
		}

		/// <summary>
		/// Sets the name of the button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="index">Index.</param>
		protected virtual void SetButtonData(TButton button, int index)
		{
			// button.SetData(tabObjects[index]);
		}

		/// <summary>
		/// Disable the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void DisableTab(TTab tab)
		{
			var i = Array.IndexOf(TabObjects, tab);
			if (i != -1)
			{
				DefaultButtons[i].interactable = false;
				ActiveButtons[i].interactable = false;
			}
		}

		/// <summary>
		/// Enable the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void EnableTab(TTab tab)
		{
			var i = Array.IndexOf(TabObjects, tab);
			if (i != -1)
			{
				DefaultButtons[i].interactable = true;
				ActiveButtons[i].interactable = true;
			}
		}

#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="styleTyped">Style for the tabs.</param>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(StyleTabs styleTyped, Style style)
		{
			if (DefaultTabButton != null)
			{
				styleTyped.DefaultButton.ApplyTo(DefaultTabButton.gameObject);

				for (int i = 0; i < DefaultButtons.Count; i++)
				{
					styleTyped.DefaultButton.ApplyTo(DefaultButtons[i].gameObject);
				}
			}

			if (ActiveTabButton != null)
			{
				styleTyped.ActiveButton.ApplyTo(ActiveTabButton.gameObject);

				for (int i = 0; i < ActiveButtons.Count; i++)
				{
					styleTyped.ActiveButton.ApplyTo(ActiveButtons[i].gameObject);
				}
			}

			foreach (var tab in tabObjects)
			{
				if (tab.TabObject != null)
				{
					styleTyped.ContentBackground.ApplyTo(tab.TabObject.GetComponent<Image>());
					style.ApplyForChildren(tab.TabObject);
				}
			}

			return true;
		}
#endregion
	}
}