namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading;
	using EasyLayoutNS;
	using UIWidgets.Attributes;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for custom ListViews.
	/// </summary>
	/// <typeparam name="TComponent">Type of DefaultItem component.</typeparam>
	/// <typeparam name="TItem">Type of item.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed.")]
	[DataBindSupport]
	public partial class ListViewCustom<TComponent, TItem> : ListViewBase
		where TComponent : ListViewItem
	{
		/// <summary>
		/// Virtualization.
		/// </summary>
		[SerializeField]
		protected bool virtualization = true;

		/// <summary>
		/// Virtualization.
		/// </summary>
		public bool Virtualization
		{
			get
			{
				return virtualization;
			}

			set
			{
				if (virtualization != value)
				{
					virtualization = value;
					UpdateView();
				}
			}
		}

		/// <summary>
		/// Disable ScrollRect if ListView is not interactable.
		/// </summary>
		[SerializeField]
		[Tooltip("Disable ScrollRect if not interactable.")]
		protected bool disableScrollRect = false;

		/// <summary>
		/// Disable ScrollRect if not interactable.
		/// </summary>
		public bool DisableScrollRect
		{
			get
			{
				return disableScrollRect;
			}

			set
			{
				if (disableScrollRect != value)
				{
					disableScrollRect = value;
					ToggleScrollRect();
				}
			}
		}

		/// <summary>
		/// ListView display type.
		/// </summary>
		[SerializeField]
		protected ListViewType listType = ListViewType.ListViewWithFixedSize;

		/// <summary>
		/// ListView display type.
		/// </summary>
		public ListViewType ListType
		{
			get
			{
				return listType;
			}

			set
			{
				listType = value;

				if (listRenderer != null)
				{
					listRenderer.Disable();
					listRenderer = null;
				}

				if (isListViewCustomInited)
				{
					SetDefaultItem(defaultItem);
				}
			}
		}

		/// <summary>
		/// The items.
		/// </summary>
		[SerializeField]
		protected List<TItem> customItems = new List<TItem>();

		/// <summary>
		/// Data source.
		/// </summary>
		#if UNITY_2020_1_OR_NEWER
		[NonSerialized]
		#endif
		protected ObservableList<TItem> dataSource;

		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>The data source.</value>
		[DataBindField]
		public virtual ObservableList<TItem> DataSource
		{
			get
			{
				if (dataSource == null)
				{
					#pragma warning disable 0618
					dataSource = new ObservableList<TItem>(customItems);
					dataSource.OnChange += UpdateItems;
					customItems = null;
					#pragma warning restore 0618
				}

				if (!isListViewCustomInited)
				{
					Init();
				}

				return dataSource;
			}

			set
			{
				if (!isListViewCustomInited)
				{
					Init();
				}

				SetNewItems(value, IsMainThread);

				if (IsMainThread)
				{
					ListRenderer.SetPosition(0f);
				}
				else
				{
					DataSourceSetted = true;
				}
			}
		}

		/// <summary>
		/// If data source setted?
		/// </summary>
		protected bool DataSourceSetted = false;

		/// <summary>
		/// Is data source changed?
		/// </summary>
		protected bool IsDataSourceChanged = false;

		[SerializeField]
		[FormerlySerializedAs("DefaultItem")]
		TComponent defaultItem;

		/// <summary>
		/// The default item template.
		/// </summary>
		public TComponent DefaultItem
		{
			get
			{
				return defaultItem;
			}

			set
			{
				SetDefaultItem(value);
			}
		}

		#region ComponentPool fields

		/// <summary>
		/// The components list.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> Components = new List<TComponent>();

		/// <summary>
		/// The components cache list.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> ComponentsCache = new List<TComponent>();

		/// <summary>
		/// The components displayed indices.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<int> ComponentsDisplayedIndices = new List<int>();

		/// <summary>
		/// Destroy instances of the previous DefaultItem when replacing DefaultItem.
		/// </summary>
		[SerializeField]
		[Tooltip("Destroy instances of the previous DefaultItem when replacing DefaultItem.")]
		protected bool destroyDefaultItemsCache = true;

		/// <summary>
		/// Destroy instances of the previous DefaultItem when replacing DefaultItem.
		/// </summary>
		public bool DestroyDefaultItemsCache
		{
			get
			{
				return ComponentsPool.DestroyComponents;
			}

			set
			{
				ComponentsPool.DestroyComponents = value;
			}
		}

		ListViewComponentPool<TComponent, TItem> componentsPool;

		/// <summary>
		/// The components pool.
		/// Constructor with lists needed to avoid lost connections when instantiated copy of the inited ListView.
		/// </summary>
		protected virtual ListViewComponentPool<TComponent, TItem> ComponentsPool
		{
			get
			{
				if (componentsPool == null)
				{
					componentsPool = new ListViewComponentPool<TComponent, TItem>(Components, ComponentsCache, ComponentsDisplayedIndices)
					{
						Owner = this,
						Container = Container,
						CallbackAdd = AddCallback,
						CallbackRemove = RemoveCallback,
						Template = DefaultItem,
						DestroyComponents = destroyDefaultItemsCache,
					};
				}

				return componentsPool;
			}
		}

		#endregion

		/// <summary>
		/// The displayed indices.
		/// </summary>
		protected List<int> DisplayedIndices = new List<int>();

		/// <summary>
		/// Gets the first displayed index.
		/// </summary>
		/// <value>The first displayed index.</value>
		[Obsolete("Renamed to DisplayedIndexFirst.")]
		public int DisplayedIndicesFirst
		{
			get
			{
				return DisplayedIndices.Count > 0 ? DisplayedIndices[0] : -1;
			}
		}

		/// <summary>
		/// Gets the last displayed index.
		/// </summary>
		/// <value>The last displayed index.</value>
		[Obsolete("Renamed to DisplayedIndexLast.")]
		public int DisplayedIndicesLast
		{
			get
			{
				return DisplayedIndices.Count > 0 ? DisplayedIndices[DisplayedIndices.Count - 1] : -1;
			}
		}

		/// <summary>
		/// Gets the first displayed index.
		/// </summary>
		/// <value>The first displayed index.</value>
		public int DisplayedIndexFirst
		{
			get
			{
				return DisplayedIndices.Count > 0 ? DisplayedIndices[0] : -1;
			}
		}

		/// <summary>
		/// Gets the last displayed index.
		/// </summary>
		/// <value>The last displayed index.</value>
		public int DisplayedIndexLast
		{
			get
			{
				return DisplayedIndices.Count > 0 ? DisplayedIndices[DisplayedIndices.Count - 1] : -1;
			}
		}

		/// <summary>
		/// Gets the selected item.
		/// </summary>
		/// <value>The selected item.</value>
		[DataBindField]
		public TItem SelectedItem
		{
			get
			{
				if (SelectedIndex == -1)
				{
					return default(TItem);
				}

				return DataSource[SelectedIndex];
			}
		}

		/// <summary>
		/// Gets the selected items.
		/// </summary>
		/// <value>The selected items.</value>
		[DataBindField]
		public List<TItem> SelectedItems
		{
			get
			{
				return SelectedIndices.Convert<int, TItem>(GetDataItem);
			}
		}

		/// <summary>
		/// If enabled scroll limited to last item.
		/// </summary>
		[SerializeField]
		[Obsolete("Use ScrollRect.MovementType = Clamped instead.")]
		public bool LimitScrollValue = false;

		[SerializeField]
		[FormerlySerializedAs("Sort")]
		bool sort = true;

		/// <summary>
		/// Sort items.
		/// Deprecated. Replaced with DataSource.Comparison.
		/// </summary>
		[Obsolete("Replaced with DataSource.Comparison.")]
		public virtual bool Sort
		{
			get
			{
				return sort;
			}

			set
			{
				sort = value;
				if (sort && isListViewCustomInited)
				{
					UpdateItems();
				}
			}
		}

		[Obsolete("Replaced with DataSource.Comparison.")]
		Func<IEnumerable<TItem>, IEnumerable<TItem>> sortFunc;

		/// <summary>
		/// Sort function.
		/// Deprecated. Replaced with DataSource.Comparison.
		/// </summary>
		[Obsolete("Replaced with DataSource.Comparison.")]
		public Func<IEnumerable<TItem>, IEnumerable<TItem>> SortFunc
		{
			get
			{
				return sortFunc;
			}

			set
			{
				sortFunc = value;
				if (Sort && isListViewCustomInited)
				{
					UpdateItems();
				}
			}
		}

		/// <summary>
		/// What to do when the object selected.
		/// </summary>
		[DataBindEvent("SelectedItem", "SelectedItems")]
		[SerializeField]
		public ListViewCustomEvent OnSelectObject = new ListViewCustomEvent();

		/// <summary>
		/// What to do when the object deselected.
		/// </summary>
		[DataBindEvent("SelectedItem", "SelectedItems")]
		[SerializeField]
		public ListViewCustomEvent OnDeselectObject = new ListViewCustomEvent();

		/// <summary>
		/// What to do when the event system send a pointer enter Event.
		/// </summary>
		[SerializeField]
		public ListViewCustomEvent OnPointerEnterObject = new ListViewCustomEvent();

		/// <summary>
		/// What to do when the event system send a pointer exit Event.
		/// </summary>
		[SerializeField]
		public ListViewCustomEvent OnPointerExitObject = new ListViewCustomEvent();

		#region Coloring fields

		[SerializeField]
		bool allowColoring = true;

		/// <summary>
		/// Allow items coloring.
		/// </summary>
		public bool AllowColoring
		{
			get
			{
				return allowColoring;
			}

			set
			{
				if (allowColoring != value)
				{
					allowColoring = value;
					ComponentsColoring(true);
				}
			}
		}

		[SerializeField]
		Color defaultBackgroundColor = Color.white;

		[SerializeField]
		Color defaultColor = Color.black;

		/// <summary>
		/// Default background color.
		/// </summary>
		public Color DefaultBackgroundColor
		{
			get
			{
				return defaultBackgroundColor;
			}

			set
			{
				defaultBackgroundColor = value;
				ComponentsColoring(true);
			}
		}

		/// <summary>
		/// Default text color.
		/// </summary>
		public Color DefaultColor
		{
			get
			{
				return defaultColor;
			}

			set
			{
				defaultColor = value;
				ComponentsColoring(true);
			}
		}

		[SerializeField]
		[FormerlySerializedAs("HighlightedBackgroundColor")]
		Color highlightedBackgroundColor = new Color(203, 230, 244, 255);

		/// <summary>
		/// Color of background on pointer over.
		/// </summary>
		public Color HighlightedBackgroundColor
		{
			get
			{
				return highlightedBackgroundColor;
			}

			set
			{
				highlightedBackgroundColor = value;
				ComponentsHighlightedColoring();
			}
		}

		/// <summary>
		/// Color of text on pointer text.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("HighlightedColor")]
		Color highlightedColor = Color.black;

		/// <summary>
		/// Color of background on pointer over.
		/// </summary>
		public Color HighlightedColor
		{
			get
			{
				return highlightedColor;
			}

			set
			{
				highlightedColor = value;
				ComponentsHighlightedColoring();
			}
		}

		[SerializeField]
		Color selectedBackgroundColor = new Color(53, 83, 227, 255);

		[SerializeField]
		Color selectedColor = Color.black;

		/// <summary>
		/// Background color of selected item.
		/// </summary>
		public Color SelectedBackgroundColor
		{
			get
			{
				return selectedBackgroundColor;
			}

			set
			{
				selectedBackgroundColor = value;
				ComponentsColoring(true);
			}
		}

		/// <summary>
		/// Text color of selected item.
		/// </summary>
		public Color SelectedColor
		{
			get
			{
				return selectedColor;
			}

			set
			{
				selectedColor = value;
				ComponentsColoring(true);
			}
		}

		/// <summary>
		/// How long a color transition should take.
		/// </summary>
		[SerializeField]
		public float FadeDuration = 0f;
		#endregion

		/// <summary>
		/// The ScrollRect.
		/// </summary>
		[SerializeField]
		protected ScrollRect scrollRect;

		/// <summary>
		/// Gets or sets the ScrollRect.
		/// </summary>
		/// <value>The ScrollRect.</value>
		public ScrollRect ScrollRect
		{
			get
			{
				return scrollRect;
			}

			set
			{
				if (scrollRect != null)
				{
					var r = scrollRect.GetComponent<ResizeListener>();
					if (r != null)
					{
						r.OnResize.RemoveListener(SetNeedResize);
					}

					scrollRect.onValueChanged.RemoveListener(SelectableCheck);
					ListRenderer.Disable();
					scrollRect.onValueChanged.RemoveListener(SelectableSet);
					scrollRect.onValueChanged.RemoveListener(OnScrollRectUpdate);
				}

				scrollRect = value;

				if (scrollRect != null)
				{
					var resizeListener = Utilities.GetOrAddComponent<ResizeListener>(scrollRect);
					resizeListener.OnResize.AddListener(SetNeedResize);

					scrollRect.onValueChanged.AddListener(SelectableCheck);
					ListRenderer.Enable();
					scrollRect.onValueChanged.AddListener(SelectableSet);
					scrollRect.onValueChanged.AddListener(OnScrollRectUpdate);

					UpdateScrollRectSize();
				}
			}
		}

		#region ListRenderer fields

		/// <summary>
		/// The DefaultItem layout group.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected LayoutGroup DefaultItemLayoutGroup;

		/// <summary>
		/// The DefaultItem layout group.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected LayoutGroup DefaultItemLayout;

		/// <summary>
		/// The layout elements of the DefaultItem.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<ILayoutElement> LayoutElements = new List<ILayoutElement>();

		[SerializeField]
		[HideInInspector]
		TComponent defaultItemCopy;

		/// <summary>
		/// Gets the default item copy.
		/// </summary>
		/// <value>The default item copy.</value>
		protected TComponent DefaultItemCopy
		{
			get
			{
				if (defaultItemCopy == null)
				{
					defaultItemCopy = Compatibility.Instantiate(DefaultItem);
					defaultItemCopy.transform.SetParent(DefaultItem.transform.parent, false);
					defaultItemCopy.gameObject.name = "DefaultItemCopy";
					defaultItemCopy.gameObject.SetActive(false);

					Utilities.FixInstantiated(DefaultItem, defaultItemCopy);
				}

				return defaultItemCopy;
			}
		}

		RectTransform defaultItemCopyRect;

		/// <summary>
		/// Gets the RectTransform of DefaultItemCopy.
		/// </summary>
		/// <value>RectTransform.</value>
		protected RectTransform DefaultItemCopyRect
		{
			get
			{
				if (defaultItemCopyRect == null)
				{
					defaultItemCopyRect = defaultItemCopy.transform as RectTransform;
				}

				return defaultItemCopyRect;
			}
		}
		#endregion

		[SerializeField]
		[HideInInspector]
		ListViewTypeBase listRenderer;

		/// <summary>
		/// ListView renderer.
		/// </summary>
		protected ListViewTypeBase ListRenderer
		{
			get
			{
				if (listRenderer == null)
				{
					listRenderer = GetRenderer(ListType);
				}

				return listRenderer;
			}

			set
			{
				listRenderer = value;
			}
		}

		/// <summary>
		/// Maximal count of the visible items.
		/// </summary>
		public int MaxVisibleItems
		{
			get
			{
				Init();

				return ListRenderer.MaxVisibleItems;
			}
		}

		/// <summary>
		/// The size of the DefaultItem.
		/// </summary>
		protected Vector2 ItemSize;

		/// <summary>
		/// The size of the ScrollRect.
		/// </summary>
		protected Vector2 ScrollRectSize;

		/// <summary>
		/// The direction.
		/// </summary>
		[SerializeField]
		protected ListViewDirection direction = ListViewDirection.Vertical;

		/// <summary>
		/// Set content size fitter settings?
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("_setContentSizeFitter")]
		protected bool setContentSizeFitter = true;

		/// <summary>
		/// The set ContentSizeFitter parameters according direction.
		/// </summary>
		public bool SetContentSizeFitter
		{
			get
			{
				return setContentSizeFitter;
			}

			set
			{
				setContentSizeFitter = value;
				if (LayoutBridge != null)
				{
					LayoutBridge.UpdateContentSizeFitter = value && ListRenderer.AllowSetContentSizeFitter;
				}
			}
		}

		/// <summary>
		/// Gets or sets the direction.
		/// </summary>
		/// <value>The direction.</value>
		public ListViewDirection Direction
		{
			get
			{
				return direction;
			}

			set
			{
				SetDirection(value);
			}
		}

		[NonSerialized]
		bool isListViewCustomInited = false;

		/// <summary>
		/// The layout.
		/// </summary>
		EasyLayout layout;

		/// <summary>
		/// Gets the layout.
		/// </summary>
		/// <value>The layout.</value>
		public EasyLayout Layout
		{
			get
			{
				if (layout == null)
				{
					layout = Container.GetComponent<EasyLayout>();
				}

				return layout;
			}
		}

		/// <summary>
		/// Selected items cache (to keep valid selected indices with updates).
		/// </summary>
		protected HashSet<TItem> SelectedItemsCache = new HashSet<TItem>();

		ILayoutBridge layoutBridge;

		/// <summary>
		/// Scroll use unscaled time.
		/// </summary>
		[SerializeField]
		public bool ScrollUnscaledTime = true;

		/// <summary>
		/// Scroll movement curve.
		/// </summary>
		[SerializeField]
		[Tooltip("Requirements: start value should be less than end value; Recommended start value = 0; end value = 1;")]
		public AnimationCurve ScrollMovement = AnimationCurve.EaseInOut(0, 0, 0.25f, 1);

		/// <summary>
		/// The scroll coroutine.
		/// </summary>
		protected IEnumerator ScrollCoroutine;

		/// <summary>
		/// LayoutBridge.
		/// </summary>
		protected ILayoutBridge LayoutBridge
		{
			get
			{
				if ((layoutBridge == null) && ListRenderer.IsVirtualizationPossible())
				{
					if (Layout != null)
					{
						layoutBridge = new EasyLayoutBridge(Layout, DefaultItem.transform as RectTransform, setContentSizeFitter && ListRenderer.AllowSetContentSizeFitter, ListRenderer.AllowControlRectTransform)
						{
							IsHorizontal = IsHorizontal(),
						};
						ListRenderer.DirectionChanged();
					}
					else
					{
						var hv_layout = Container.GetComponent<HorizontalOrVerticalLayoutGroup>();
						if (hv_layout != null)
						{
							layoutBridge = new StandardLayoutBridge(hv_layout, DefaultItem.transform as RectTransform, setContentSizeFitter && ListRenderer.AllowSetContentSizeFitter);
						}
					}
				}

				return layoutBridge;
			}
		}

		/// <summary>
		/// The main thread.
		/// </summary>
		protected Thread MainThread;

		/// <summary>
		/// Gets a value indicating whether this instance is executed in main thread.
		/// </summary>
		/// <value><c>true</c> if this instance is executed in main thread; otherwise, <c>false</c>.</value>
		protected bool IsMainThread
		{
			get
			{
				return MainThread != null && MainThread.Equals(Thread.CurrentThread);
			}
		}

		/// <summary>
		/// Is DefaultItem implements IViewData{TItem}.
		/// </summary>
		protected bool CanSetData;

		/// <summary>
		/// Center the list items if all items visible.
		/// </summary>
		[SerializeField]
		[Tooltip("Center the list items if all items visible.")]
		protected bool centerTheItems;

		/// <summary>
		/// Center the list items if all items visible.
		/// </summary>
		public virtual bool CenterTheItems
		{
			get
			{
				return centerTheItems;
			}

			set
			{
				centerTheItems = value;
				UpdateView();
			}
		}

		/// <summary>
		/// List should be looped.
		/// </summary>
		[SerializeField]
		protected bool loopedList = false;

		/// <summary>
		/// List can be looped.
		/// </summary>
		/// <value><c>true</c> if list can be looped; otherwise, <c>false</c>.</value>
		public virtual bool LoopedList
		{
			get
			{
				return loopedList;
			}

			set
			{
				loopedList = value;
			}
		}

		/// <summary>
		/// List can be looped and items is enough to make looped list.
		/// </summary>
		/// <value><c>true</c> if looped list available; otherwise, <c>false</c>.</value>
		public virtual bool LoopedListAvailable
		{
			get
			{
				return LoopedList && Virtualization && ListRenderer.IsVirtualizationSupported() && ListRenderer.AllowLoopedList;
			}
		}

		/// <summary>
		/// Precalculate item sizes.
		/// Disabling this option increase performance with huge lists of items with variable sizes and decrease scroll precision.
		/// </summary>
		[SerializeField]
		public bool PrecalculateItemSizes = true;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isListViewCustomInited)
			{
				return;
			}

			isListViewCustomInited = true;

			MainThread = Thread.CurrentThread;

			base.Init();
			Items = new List<ListViewItem>();

			SelectedItemsCache.Clear();
			for (int i = 0; i < SelectedIndices.Count; i++)
			{
				var index = SelectedIndices[i];
				SelectedItemsCache.Add(DataSource[index]);
			}

			DestroyGameObjects = false;

			CanSetData = DefaultItem is IViewData<TItem>;

			ComponentsPool.Template = defaultItem;

			DefaultItem.gameObject.SetActive(true);
			DefaultItem.FindSelectableObjects();

			if (ListRenderer.IsVirtualizationSupported())
			{
				ScrollRect = scrollRect;
				CalculateItemSize();
			}

			SetContentSizeFitter = setContentSizeFitter;

			DefaultItem.gameObject.SetActive(false);

			SetDirection(direction);

			UpdateItems();
		}

		/// <summary>
		/// Require EasyLayout.
		/// </summary>
		protected virtual bool RequireEasyLayout
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Update ScrollRect size.
		/// </summary>
		protected void UpdateScrollRectSize()
		{
			ScrollRectSize = (scrollRect.transform as RectTransform).rect.size;
			ScrollRectSize.x = float.IsNaN(ScrollRectSize.x) ? 1f : Mathf.Max(ScrollRectSize.x, 1f);
			ScrollRectSize.y = float.IsNaN(ScrollRectSize.y) ? 1f : Mathf.Max(ScrollRectSize.y, 1f);
		}

		/// <summary>
		/// Get the rendered of the specified ListView type.
		/// </summary>
		/// <param name="type">ListView type</param>
		/// <returns>Renderer.</returns>
		protected virtual ListViewTypeBase GetRenderer(ListViewType type)
		{
			ListViewTypeBase renderer;
			switch (type)
			{
				case ListViewType.ListViewWithFixedSize:
					renderer = new ListViewTypeFixed(this);
					break;
				case ListViewType.ListViewWithVariableSize:
					renderer = new ListViewTypeSize(this);
					break;
				case ListViewType.TileViewWithFixedSize:
					renderer = new TileViewTypeFixed(this);
					break;
				case ListViewType.TileViewWithVariableSize:
					renderer = new TileViewTypeSize(this);
					break;
				case ListViewType.TileViewStaggered:
					renderer = new TileViewStaggered(this);
					break;
				case ListViewType.ListViewEllipse:
					renderer = new ListViewTypeEllipse(this);
					break;
				default:
					throw new NotSupportedException("Unknown ListView type: " + type);
			}

			renderer.Enable();

			return renderer;
		}

		/// <summary>
		/// Sets the default item.
		/// </summary>
		/// <param name="newDefaultItem">New default item.</param>
		protected virtual void SetDefaultItem(TComponent newDefaultItem)
		{
			if (newDefaultItem == null)
			{
				throw new ArgumentNullException("newDefaultItem");
			}

			if (defaultItemCopy != null)
			{
				Destroy(defaultItemCopy.gameObject);
				defaultItemCopy = null;
				defaultItemCopyRect = null;
			}

			defaultItem = newDefaultItem;

			if (!isListViewCustomInited)
			{
				return;
			}

			defaultItem.gameObject.SetActive(true);
			defaultItem.FindSelectableObjects();
			CalculateItemSize(true);

			CanSetData = defaultItem is IViewData<TItem>;

			ComponentsPool.Template = defaultItem;

			CalculateMaxVisibleItems();

			UpdateView();

			if (scrollRect != null)
			{
				var resizeListener = scrollRect.GetComponent<ResizeListener>();
				if (resizeListener != null)
				{
					resizeListener.OnResize.Invoke();
				}
			}
		}

		#region ComponentsHighlightedColoring

		readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

		readonly List<int> highlightedIndices = new List<int>(2);

		/// <summary>
		/// Highlighted indices.
		/// </summary>
		public ReadOnlyCollection<int> HighlightedIndices
		{
			get
			{
				highlightedIndices.Clear();

				var item_under_navigation = FindItem(EventSystem.current.currentSelectedGameObject, Container);
				if (item_under_navigation != null)
				{
					highlightedIndices.Add(item_under_navigation.Index);
				}

				var item_under_pointer = FindItem();
				if ((item_under_pointer != null) && (!highlightedIndices.Contains(item_under_pointer.Index)))
				{
					highlightedIndices.Add(item_under_pointer.Index);
				}

				return highlightedIndices.AsReadOnly();
			}
		}

		/// <summary>
		/// Apply changed highlighted colors.
		/// </summary>
		protected void ComponentsHighlightedColoring()
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			foreach (var index in HighlightedIndices)
			{
				HighlightColoring(GetComponent(index));
			}
		}

		ListViewItem FindItem()
		{
			raycastResults.Clear();

			Vector2 position;
			var canvas = Utilities.FindTopmostCanvas(transform);
			if (canvas == null)
			{
				return null;
			}

			var root = canvas.transform as RectTransform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(root, Input.mousePosition, null, out position);
			position.x += root.rect.width * root.pivot.x;
			position.y += root.rect.height * root.pivot.y;

			var event_data = new PointerEventData(EventSystem.current)
			{
				position = position,
			};

			EventSystem.current.RaycastAll(event_data, raycastResults);

			foreach (var raycastResult in raycastResults)
			{
				if (!raycastResult.isValid)
				{
					continue;
				}

				var item = raycastResult.gameObject.GetComponent<ListViewItem>();
				if ((item != null) && (item.Owner.GetInstanceID() == GetInstanceID()))
				{
					return item;
				}
			}

			return null;
		}

		static ListViewItem FindItem(GameObject go, Transform parent)
		{
			if (go == null)
			{
				return null;
			}

			if (!go.transform.IsChildOf(parent))
			{
				return null;
			}

			var t = go.transform;
			while ((t.parent != null) && (t.parent.GetInstanceID() != parent.GetInstanceID()))
			{
				t = t.parent;
			}

			return (t.parent == null) ? null : t.GetComponent<ListViewItem>();
		}
		#endregion

		/// <summary>
		/// Gets the layout margin.
		/// </summary>
		/// <returns>The layout margin.</returns>
		public override Vector4 GetLayoutMargin()
		{
			return LayoutBridge.GetMarginSize();
		}

		/// <summary>
		/// Sets the direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		protected virtual void SetDirection(ListViewDirection newDirection)
		{
			direction = newDirection;

			ListRenderer.ResetPosition();

			(Container as RectTransform).anchoredPosition = Vector2.zero;

			if (ListRenderer.IsVirtualizationSupported())
			{
				LayoutBridge.IsHorizontal = IsHorizontal();
				ListRenderer.DirectionChanged();

				CalculateMaxVisibleItems();
			}

			UpdateView();
		}

		/// <summary>
		/// Determines whether is sort enabled.
		/// </summary>
		/// <returns><c>true</c> if is sort enabled; otherwise, <c>false</c>.</returns>
		public bool IsSortEnabled()
		{
			if (DataSource.Comparison != null)
			{
				return true;
			}

#pragma warning disable 0618
			return Sort && SortFunc != null;
#pragma warning restore 0618
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest index.</returns>
		/// <param name="eventData">Event data.</param>
		/// <param name="type">Preferable nearest index.</param>
		public override int GetNearestIndex(PointerEventData eventData, NearestType type)
		{
			if (IsSortEnabled())
			{
				return -1;
			}

			Vector2 point;
			var rectTransform = Container as RectTransform;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out point))
			{
				return DataSource.Count;
			}

			var rect = rectTransform.rect;
			if (!rect.Contains(point))
			{
				return DataSource.Count;
			}

			return GetNearestIndex(point, type);
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest item index.</returns>
		/// <param name="point">Point.</param>
		/// <param name="type">Preferable nearest index.</param>
		public override int GetNearestIndex(Vector2 point, NearestType type)
		{
			var index = ListRenderer.GetNearestIndex(point, type);
			if (ListRenderer.AllowLoopedList)
			{
				index = ListRenderer.VisibleIndex2ItemIndex(index);
			}

			return index;
		}

		/// <summary>
		/// Gets the spacing between items.
		/// </summary>
		/// <returns>The item spacing.</returns>
		public override float GetItemSpacing()
		{
			return LayoutBridge.GetSpacing();
		}

		/// <summary>
		/// Gets the horizontal spacing between items.
		/// </summary>
		/// <returns>The item spacing.</returns>
		public override float GetItemSpacingX()
		{
			return LayoutBridge.GetSpacingX();
		}

		/// <summary>
		/// Gets the vertical spacing between items.
		/// </summary>
		/// <returns>The item spacing.</returns>
		public override float GetItemSpacingY()
		{
			return LayoutBridge.GetSpacingY();
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="index">Index.</param>
		protected TItem GetDataItem(int index)
		{
			return DataSource[index];
		}

		/// <summary>
		/// Calculates the size of the item.
		/// </summary>
		/// <param name="reset">Reset item size.</param>
		protected virtual void CalculateItemSize(bool reset = false)
		{
			ItemSize = ListRenderer.GetItemSize(reset);
		}

		/// <summary>
		/// Determines whether this instance is horizontal.
		/// </summary>
		/// <returns><c>true</c> if this instance is horizontal; otherwise, <c>false</c>.</returns>
		public override bool IsHorizontal()
		{
			return direction == ListViewDirection.Horizontal;
		}

		/// <summary>
		/// Calculates the max count of visible items.
		/// </summary>
		protected virtual void CalculateMaxVisibleItems()
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			ListRenderer.CalculateMaxVisibleItems();

			ListRenderer.ValidateContentSize();
		}

		/// <summary>
		/// Resize this instance.
		/// </summary>
		public virtual void Resize()
		{
			ListRenderer.CalculateItemsSizes(DataSource, false);

			NeedResize = false;

			UpdateScrollRectSize();

			CalculateItemSize(true);
			CalculateMaxVisibleItems();
			UpdateView();
		}

		/// <summary>
		/// Invokes the select event.
		/// </summary>
		/// <param name="index">Index.</param>
		protected override void InvokeSelect(int index)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			var component = GetComponent(index);
			var item = DataSource[index];

			base.InvokeSelect(index);

			SelectedItemsCache.Add(item);
			OnSelectObject.Invoke(index);

			SelectColoring(component);
			if (component != null)
			{
				component.StateSelected();
			}
		}

		/// <summary>
		/// Invokes the deselect event.
		/// </summary>
		/// <param name="index">Index.</param>
		protected override void InvokeDeselect(int index)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			var component = GetComponent(index);
			var item = DataSource[index];

			base.InvokeDeselect(index);

			SelectedItemsCache.Remove(item);
			OnDeselectObject.Invoke(index);

			DefaultColoring(component);
			if (component != null)
			{
				component.StateDefault();
			}
		}

		/// <summary>
		/// Process the pointer enter callback event.
		/// </summary>
		/// <param name="item">Item.</param>
		void OnPointerEnterCallback(ListViewItem item)
		{
			OnPointerEnterObject.Invoke(item.Index);

			if (!IsSelected(item.Index))
			{
				HighlightColoring(item);
				item.StateHighlighted();
			}
		}

		/// <summary>
		/// Process the pointer exit callback event.
		/// </summary>
		/// <param name="item">Item.</param>
		void OnPointerExitCallback(ListViewItem item)
		{
			OnPointerExitObject.Invoke(item.Index);

			if (!IsSelected(item.Index))
			{
				DefaultColoring(item);
				item.StateDefault();
			}
		}

		/// <summary>
		/// Set flag to update view when data source changed.
		/// </summary>
		public override void UpdateItems()
		{
			SetNewItems(DataSource, IsMainThread);
			IsDataSourceChanged = !IsMainThread;
		}

		/// <summary>
		/// Clear items of this instance.
		/// </summary>
		public override void Clear()
		{
			DataSource.Clear();
			ListRenderer.SetPosition(0f);
		}

		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of added item.</returns>
		public virtual int Add(TItem item)
		{
			DataSource.Add(item);

			return DataSource.IndexOf(item);
		}

		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of removed TItem.</returns>
		public virtual int Remove(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index == -1)
			{
				return index;
			}

			DataSource.RemoveAt(index);

			return index;
		}

		/// <summary>
		/// Remove item by the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public virtual void Remove(int index)
		{
			DataSource.RemoveAt(index);
		}

		/// <summary>
		/// Scrolls to specified item immediately.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void ScrollTo(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index > -1)
			{
				ScrollTo(index);
			}
		}

		/// <summary>
		/// Scroll to the specified item with animation.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void ScrollToAnimated(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index > -1)
			{
				ScrollToAnimated(index);
			}
		}

		/// <summary>
		/// Scrolls to item with specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public override void ScrollTo(int index)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(ListRenderer.GetPosition(index));
		}

		/// <summary>
		/// Get scroll position.
		/// </summary>
		/// <returns>Position.</returns>
		public override float GetScrollPosition()
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return 0f;
			}

			return ListRenderer.GetPosition();
		}

		/// <summary>
		/// Scrolls to specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public override void ScrollToPosition(float position)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(position);
		}

		/// <summary>
		/// Scrolls to specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public override void ScrollToPosition(Vector2 position)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(position);
		}

		/// <summary>
		/// Get secondary scroll position (for the cross direction).
		/// </summary>
		/// <param name="index">Index.</param>
		/// <returns>Secondary scroll position.</returns>
		protected virtual float GetScrollPositionSecondary(int index)
		{
			var current_position = scrollRect.content.anchoredPosition;

			return IsHorizontal() ? current_position.y : current_position.x;
		}

		/// <summary>
		/// Is visible item with specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="minVisiblePart">The minimal visible part of the item to consider item visible.</param>
		/// <returns>true if item visible; false otherwise.</returns>
		public override bool IsVisible(int index, float minVisiblePart = 0f)
		{
			if (!ListRenderer.IsVirtualizationSupported())
			{
				return false;
			}

			return ListRenderer.IsVisible(index, minVisiblePart);
		}

		/// <summary>
		/// Starts the scroll coroutine.
		/// </summary>
		/// <param name="coroutine">Coroutine.</param>
		protected virtual void StartScrollCoroutine(IEnumerator coroutine)
		{
			StopScrollCoroutine();
			ScrollCoroutine = coroutine;
			StartCoroutine(ScrollCoroutine);
		}

		/// <summary>
		/// Stops the scroll coroutine.
		/// </summary>
		protected virtual void StopScrollCoroutine()
		{
			if (ScrollCoroutine != null)
			{
				StopCoroutine(ScrollCoroutine);
			}
		}

		/// <summary>
		/// Stop scrolling.
		/// </summary>
		public override void ScrollStop()
		{
			StopScrollCoroutine();
		}

		/// <summary>
		/// Scroll to specified index with time.
		/// </summary>
		/// <param name="index">Index.</param>
		public override void ScrollToAnimated(int index)
		{
			StartScrollCoroutine(ScrollToAnimatedCoroutine(index, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scrolls to specified position with time.
		/// </summary>
		/// <param name="target">Position.</param>
		public override void ScrollToPositionAnimated(float target)
		{
			var current_position = ListRenderer.GetPositionVector();
			var target_position = IsHorizontal()
				? new Vector2(ListRenderer.ValidatePosition(-target), current_position.y)
				: new Vector2(current_position.x, ListRenderer.ValidatePosition(target));

#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				return target_position;
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			StartScrollCoroutine(ScrollToAnimatedCoroutine(Position, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scrolls to specified position with time.
		/// </summary>
		/// <param name="target">Position.</param>
		public override void ScrollToPositionAnimated(Vector2 target)
		{
			target = ListRenderer.ValidatePosition(target);

#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				return target;
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			StartScrollCoroutine(ScrollToAnimatedCoroutine(Position, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scroll to specified index with time coroutine.
		/// </summary>
		/// <returns>The scroll to index with time coroutine.</returns>
		/// <param name="index">Index.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		protected virtual IEnumerator ScrollToAnimatedCoroutine(int index, bool unscaledTime)
		{
#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				return ListRenderer.GetPosition(index);
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			return ScrollToAnimatedCoroutine(Position, unscaledTime);
		}

		/// <summary>
		/// Get start position for the animated scroll.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <returns>Start position.</returns>
		protected virtual Vector2 GetScrollStartPosition(Vector2 target)
		{
			var start = ListRenderer.GetPositionVector();
			if (IsHorizontal())
			{
				start.x = -start.x;
			}

			if (ListRenderer.AllowLoopedList)
			{
				// find shortest distance to target for the looped list
				var list_size = ListRenderer.ListSize() + LayoutBridge.GetSpacing();
				var distance_straight = IsHorizontal()
					? (target.x - start.x)
					: (target.y - start.y);
				var distance_reverse_1 = IsHorizontal()
					? (target.x - (start.x + list_size))
					: (target.y - start.y + list_size);
				var distance_reverse_2 = IsHorizontal()
					? (target.x - (start.x - list_size))
					: (target.y - start.y - list_size);

				if (Mathf.Abs(distance_reverse_1) < Mathf.Abs(distance_straight))
				{
					if (IsHorizontal())
					{
						start.x += list_size;
					}
					else
					{
						start.y += list_size;
					}
				}

				if (Mathf.Abs(distance_reverse_2) < Mathf.Abs(distance_straight))
				{
					if (IsHorizontal())
					{
						start.x -= list_size;
					}
					else
					{
						start.y -= list_size;
					}
				}
			}

			return start;
		}

		/// <summary>
		/// Scroll to specified position with time coroutine.
		/// </summary>
		/// <returns>The scroll to index with time coroutine.</returns>
		/// <param name="targetPosition">Target position.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		protected virtual IEnumerator ScrollToAnimatedCoroutine(Func<Vector2> targetPosition, bool unscaledTime)
		{
			var start = GetScrollStartPosition(targetPosition());

			float delta;
			var animationLength = ScrollMovement.keys[ScrollMovement.keys.Length - 1].time;
			var startTime = Utilities.GetTime(unscaledTime);

			do
			{
				delta = Utilities.GetTime(unscaledTime) - startTime;
				var value = ScrollMovement.Evaluate(delta);

				var target = targetPosition();
				var pos = start + ((target - start) * value);

				ListRenderer.SetPosition(pos);

				yield return null;
			}
			while (delta < animationLength);

			ListRenderer.SetPosition(targetPosition());

			yield return null;

			ListRenderer.SetPosition(targetPosition());
		}

		/// <summary>
		/// Gets the item position by index.
		/// </summary>
		/// <returns>The item position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPosition(int index)
		{
			return ListRenderer.GetItemPosition(index);
		}

		/// <summary>
		/// Gets the item position by index.
		/// </summary>
		/// <returns>The item position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionBorderEnd(int index)
		{
			return ListRenderer.GetItemPositionBorderEnd(index);
		}

		/// <summary>
		/// Gets the item middle position by index.
		/// </summary>
		/// <returns>The item middle position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionMiddle(int index)
		{
			return ListRenderer.GetItemPositionMiddle(index);
		}

		/// <summary>
		/// Gets the item bottom position by index.
		/// </summary>
		/// <returns>The item bottom position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionBottom(int index)
		{
			return ListRenderer.GetItemPositionBottom(index);
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void AddCallback(ListViewItem item)
		{
			ListRenderer.AddCallback(item);

			item.onPointerEnterItem.AddListener(OnPointerEnterCallback);
			item.onPointerExitItem.AddListener(OnPointerExitCallback);
		}

		/// <summary>
		/// Removes the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void RemoveCallback(ListViewItem item)
		{
			if (item == null)
			{
				return;
			}

			ListRenderer.RemoveCallback(item);

			item.onPointerEnterItem.RemoveListener(OnPointerEnterCallback);
			item.onPointerExitItem.RemoveListener(OnPointerExitCallback);
		}

		/// <summary>
		/// Set the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="allowDuplicate">If set to <c>true</c> allow duplicate.</param>
		/// <returns>Index of item.</returns>
		public int Set(TItem item, bool allowDuplicate = true)
		{
			int index;

			if (!allowDuplicate)
			{
				index = DataSource.IndexOf(item);
				if (index == -1)
				{
					index = Add(item);
				}
			}
			else
			{
				index = Add(item);
			}

			Select(index);

			return index;
		}

		/// <summary>
		/// Updates the component layout.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void UpdateComponentLayout(TComponent component)
		{
			LayoutUtilities.UpdateLayoutsRecursive(component);
		}

		/// <summary>
		/// Sets component data with specified item.
		/// </summary>
		/// <param name="component">Component.</param>
		/// <param name="item">Item.</param>
		protected virtual void SetData(TComponent component, TItem item)
		{
			if (CanSetData)
			{
				(component as IViewData<TItem>).SetData(item);
			}
		}

		/// <summary>
		/// Gets the default width of the item.
		/// </summary>
		/// <returns>The default item width.</returns>
		public override float GetDefaultItemWidth()
		{
			return ItemSize.x;
		}

		/// <summary>
		/// Gets the default height of the item.
		/// </summary>
		/// <returns>The default item height.</returns>
		public override float GetDefaultItemHeight()
		{
			return ItemSize.y;
		}

		/// <summary>
		/// Sets the displayed indices.
		/// </summary>
		/// <param name="isNewData">Is new data?</param>
		protected virtual void SetDisplayedIndices(bool isNewData = true)
		{
			if (isNewData)
			{
				ComponentsPool.DisplayedIndicesSet(DisplayedIndices, ComponentSetData);
			}
			else
			{
				ComponentsPool.DisplayedIndicesUpdate(DisplayedIndices, ComponentSetData);
			}

			ListRenderer.UpdateLayout();
		}

		/// <summary>
		/// Process the ScrollRect update event.
		/// </summary>
		/// <param name="position">Position.</param>
		protected virtual void OnScrollRectUpdate(Vector2 position)
		{
			StartScrolling();
		}

		/// <summary>
		/// Set data to component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void ComponentSetData(TComponent component)
		{
			SetData(component, DataSource[component.Index]);
			Coloring(component as ListViewItem);

			if (IsSelected(component.Index))
			{
				component.StateSelected();
			}
			else
			{
				component.StateDefault();
			}
		}

		/// <summary>
		/// Updates the view.
		/// </summary>
		public virtual void UpdateView()
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			ListRenderer.UpdateDisplayedIndices();

			SetDisplayedIndices();

			OnUpdateView.Invoke();
		}

		/// <summary>
		/// Keep selected items on items update.
		/// </summary>
		[SerializeField]
		protected bool KeepSelection = true;

		/// <summary>
		/// Updates the items.
		/// </summary>
		/// <param name="newItems">New items.</param>
		/// <param name="updateView">Update view.</param>
		protected virtual void SetNewItems(ObservableList<TItem> newItems, bool updateView = true)
		{
			ListRenderer.CalculateItemsSizes(newItems, false);

			lock (DataSource)
			{
				DataSource.OnChange -= UpdateItems;

#pragma warning disable 0618
				if (Sort && SortFunc != null)
				{
					newItems.BeginUpdate();

					var sorted = new List<TItem>(SortFunc(newItems));

					newItems.Clear();
					newItems.AddRange(sorted);

					newItems.EndUpdate();
				}
#pragma warning restore 0618

				SilentDeselect(SelectedIndices);
				var new_selected_indices = RecalculateSelectedIndices(newItems);

				dataSource = newItems;

				CalculateMaxVisibleItems();

				if (KeepSelection)
				{
					SilentSelect(new_selected_indices);
				}

				SelectedItemsCache.Clear();
				for (int i = 0; i < SelectedIndices.Count; i++)
				{
					var index = SelectedIndices[i];
					SelectedItemsCache.Add(DataSource[index]);
				}

				DataSource.OnChange += UpdateItems;

				if (updateView)
				{
					UpdateView();
				}
			}
		}

		/// <summary>
		/// Recalculates the selected indices.
		/// </summary>
		/// <returns>The selected indices.</returns>
		/// <param name="newItems">New items.</param>
		protected virtual List<int> RecalculateSelectedIndices(ObservableList<TItem> newItems)
		{
			var new_selected_indices = new List<int>();

			foreach (var item in SelectedItemsCache)
			{
				var new_index = newItems.IndexOf(item);
				if (new_index != -1)
				{
					new_selected_indices.Add(new_index);
				}
			}

			return new_selected_indices;
		}

		/// <summary>
		/// Determines if item exists with the specified index.
		/// </summary>
		/// <returns><c>true</c> if item exists with the specified index; otherwise, <c>false</c>.</returns>
		/// <param name="index">Index.</param>
		public override bool IsValid(int index)
		{
			return (index >= 0) && (index < DataSource.Count);
		}

		/// <summary>
		/// Process the item move event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		/// <param name="item">Item.</param>
		protected override void OnItemMove(AxisEventData eventData, ListViewItem item)
		{
			ListRenderer.OnItemMove(eventData, item);
		}

		/// <summary>
		/// Coloring the specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected override void Coloring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				SelectColoring(component);
			}
			else
			{
				DefaultColoring(component);
			}
		}

		/// <summary>
		/// Set highlights colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected override void HighlightColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				return;
			}

			HighlightColoring(component as TComponent);
		}

		/// <summary>
		/// Set highlights colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void HighlightColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (!CanSelect(component.Index))
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				return;
			}

			component.GraphicsColoring(HighlightedColor, HighlightedBackgroundColor, FadeDuration);
		}

		/// <summary>
		/// Set select colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void SelectColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			SelectColoring(component as TComponent);
		}

		/// <summary>
		/// Set select colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void SelectColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (IsInteractable())
			{
				component.GraphicsColoring(SelectedColor, SelectedBackgroundColor, FadeDuration);
			}
			else
			{
				component.GraphicsColoring(SelectedColor * DisabledColor, SelectedBackgroundColor * DisabledColor, FadeDuration);
			}
		}

		/// <summary>
		/// Set default colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void DefaultColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			DefaultColoring(component as TComponent);
		}

		/// <summary>
		/// Set default colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void DefaultColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (IsInteractable())
			{
				component.GraphicsColoring(DefaultColor, DefaultBackgroundColor, FadeDuration);
			}
			else
			{
				component.GraphicsColoring(DefaultColor * DisabledColor, DefaultBackgroundColor * DisabledColor, FadeDuration);
			}
		}

		/// <summary>
		/// Updates the colors.
		/// </summary>
		/// <param name="instant">Is should be instant color update?</param>
		public override void ComponentsColoring(bool instant = false)
		{
			if (!allowColoring && instant)
			{
				ComponentsPool.ForEach(DefaultColoring);
				return;
			}

			if (instant)
			{
				var old_duration = FadeDuration;
				FadeDuration = 0f;
				ComponentsPool.ForEach(Coloring);
				FadeDuration = old_duration;
			}
			else
			{
				ComponentsPool.ForEach(Coloring);
			}
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			if (dataSource != null)
			{
				dataSource.OnChange -= UpdateItems;
			}

			layout = null;
			layoutBridge = null;

			ScrollRect = null;

			ComponentsPool.Template = null;

			base.OnDestroy();
		}

		/// <summary>
		/// Calls the specified action for each component.
		/// </summary>
		/// <param name="func">Action.</param>
		public override void ForEachComponent(Action<ListViewItem> func)
		{
			base.ForEachComponent(func);

			func(DefaultItem);

			if (defaultItemCopy != null)
			{
				func(DefaultItemCopy);
			}

			ComponentsPool.ForEachCache(func);
		}

		/// <summary>
		/// Calls the specified action for each component.
		/// </summary>
		/// <param name="func">Action.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "Reviewed.")]
		public virtual void ForEachComponent(Action<TComponent> func)
		{
			base.ForEachComponent<TComponent>(func);
			func(DefaultItem);
			ComponentsPool.ForEachCache(func);
		}

		/// <summary>
		/// Determines whether item visible.
		/// </summary>
		/// <returns><c>true</c> if item visible; otherwise, <c>false</c>.</returns>
		/// <param name="index">Index.</param>
		public override bool IsItemVisible(int index)
		{
			return DisplayedIndices.Contains(index);
		}

		/// <summary>
		/// Gets the visible indices.
		/// </summary>
		/// <returns>The visible indices.</returns>
		public List<int> GetVisibleIndices()
		{
			return new List<int>(DisplayedIndices);
		}

		/// <summary>
		/// Gets the visible components.
		/// </summary>
		/// <returns>The visible components.</returns>
		public List<TComponent> GetVisibleComponents()
		{
			return ComponentsPool.List();
		}

		/// <summary>
		/// Gets the item component.
		/// </summary>
		/// <returns>The item component.</returns>
		/// <param name="index">Index.</param>
		public TComponent GetItemComponent(int index)
		{
			return GetComponent(index) as TComponent;
		}

		/// <summary>
		/// OnStartScrolling event.
		/// </summary>
		public UnityEvent OnStartScrolling = new UnityEvent();

		/// <summary>
		/// OnEndScrolling event.
		/// </summary>
		public UnityEvent OnEndScrolling = new UnityEvent();

		/// <summary>
		/// Time before raise OnEndScrolling event since last OnScrollRectUpdate event raised.
		/// </summary>
		public float EndScrollDelay = 0.3f;

		/// <summary>
		/// Is ScrollRect now on scrolling state.
		/// </summary>
		protected bool Scrolling;

		/// <summary>
		/// When last scroll event happen?
		/// </summary>
		protected float LastScrollingTime;

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			if (DataSourceSetted || IsDataSourceChanged)
			{
				var reset_scroll = DataSourceSetted;

				DataSourceSetted = false;
				IsDataSourceChanged = false;

				lock (DataSource)
				{
					CalculateMaxVisibleItems();
					UpdateView();
				}

				if (reset_scroll)
				{
					ListRenderer.SetPosition(0f);
				}
			}

			if (NeedResize)
			{
				Resize();
			}

			if (IsStopScrolling())
			{
				EndScrolling();
			}

			SelectableSet();
		}

		/// <summary>
		/// LateUpdate.
		/// </summary>
		protected virtual void LateUpdate()
		{
			SelectableSet();
		}

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			StartCoroutine(ForceRebuild());

			var old = FadeDuration;
			FadeDuration = 0f;
			ComponentsPool.ForEach(Coloring);
			FadeDuration = old;

			Resize();
		}

		IEnumerator ForceRebuild()
		{
			yield return null;
			ForEachComponent(MarkLayoutForRebuild);
		}

		void MarkLayoutForRebuild(ListViewItem item)
		{
			if (item != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(item.transform as RectTransform);
			}
		}

		/// <summary>
		/// Start to track scrolling event.
		/// </summary>
		protected virtual void StartScrolling()
		{
			LastScrollingTime = Utilities.GetTime(true);
			if (Scrolling)
			{
				return;
			}

			Scrolling = true;
			OnStartScrolling.Invoke();
		}

		/// <summary>
		/// Determines whether ScrollRect is stop scrolling.
		/// </summary>
		/// <returns><c>true</c> if ScrollRect is stop scrolling; otherwise, <c>false</c>.</returns>
		protected virtual bool IsStopScrolling()
		{
			if (!Scrolling)
			{
				return false;
			}

			return (LastScrollingTime + EndScrollDelay) <= Utilities.GetTime(true);
		}

		/// <summary>
		/// Raise OnEndScrolling event.
		/// </summary>
		protected virtual void EndScrolling()
		{
			Scrolling = false;
			OnEndScrolling.Invoke();
		}

		/// <summary>
		/// Is need to handle resize event?
		/// </summary>
		protected bool NeedResize;

		/// <summary>
		/// Sets the need resize.
		/// </summary>
		protected virtual void SetNeedResize()
		{
			if (!ListRenderer.IsVirtualizationSupported())
			{
				return;
			}

			NeedResize = true;
		}

		/// <summary>
		/// Change DefaultItem size.
		/// </summary>
		/// <param name="size">Size.</param>
		public virtual void ChangeDefaultItemSize(Vector2 size)
		{
			if (defaultItemCopy != null)
			{
				var rt = defaultItemCopy.transform as RectTransform;
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
			}

			ComponentsPool.SetSize(size);

			CalculateItemSize(true);
			CalculateMaxVisibleItems();
			UpdateView();
		}

		/// <summary>
		/// Toggle ScrollRect state.
		/// </summary>
		protected void ToggleScrollRect()
		{
			if (ScrollRect != null)
			{
				ScrollRect.enabled = DisableScrollRect ? IsInteractable() : true;
			}
		}

		/// <summary>
		/// What to do when widget became interactable.
		/// </summary>
		protected override void OnInteractableEnabled()
		{
			ToggleScrollRect();
		}

		/// <summary>
		/// What to do when widget became not interactable.
		/// </summary>
		protected override void OnInteractableDisabled()
		{
			ToggleScrollRect();
		}

		#region ListViewPaginator support

		/// <summary>
		/// Gets the ScrollRect.
		/// </summary>
		/// <returns>The ScrollRect.</returns>
		public override ScrollRect GetScrollRect()
		{
			return ScrollRect;
		}

		/// <summary>
		/// Gets the items count.
		/// </summary>
		/// <returns>The items count.</returns>
		public override int GetItemsCount()
		{
			return DataSource.Count;
		}

		/// <summary>
		/// Gets the items per block count.
		/// </summary>
		/// <returns>The items per block.</returns>
		public override int GetItemsPerBlock()
		{
			return ListRenderer.GetItemsPerBlock();
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest item index.</returns>
		public override int GetNearestItemIndex()
		{
			return ListRenderer.GetNearestItemIndex();
		}

		/// <summary>
		/// Gets the size of the DefaultItem.
		/// </summary>
		/// <returns>Size.</returns>
		public override Vector2 GetDefaultItemSize()
		{
			return ItemSize;
		}
		#endregion

		#region Obsolete

		/// <summary>
		/// Gets the visible indices.
		/// </summary>
		/// <returns>The visible indices.</returns>
		[Obsolete("Use GetVisibleIndices()")]
		public List<int> GetVisibleIndicies()
		{
			return GetVisibleIndices();
		}
#endregion

#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <param name="style">Style data.</param>
		protected virtual void SetStyleDefaultItem(Style style)
		{
			if (defaultItemCopy != null)
			{
				defaultItemCopy.Owner = this;
				defaultItemCopy.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}

			if (ComponentsPool != null)
			{
				ComponentsPool.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}
		}

		/// <summary>
		/// Sets the style colors.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void SetStyleColors(Style style)
		{
			defaultBackgroundColor = style.Collections.DefaultBackgroundColor;
			defaultColor = style.Collections.DefaultColor;
			HighlightedBackgroundColor = style.Collections.HighlightedBackgroundColor;
			HighlightedColor = style.Collections.HighlightedColor;
			selectedBackgroundColor = style.Collections.SelectedBackgroundColor;
			selectedColor = style.Collections.SelectedColor;
		}

		/// <summary>
		/// Sets the ScrollRect style.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void SetStyleScrollRect(Style style)
		{
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var viewport = scrollRect.viewport != null ? scrollRect.viewport : Container.parent;
#else
			var viewport = Container.parent;
#endif
			style.Collections.Viewport.ApplyTo(viewport.GetComponent<Image>());

			style.HorizontalScrollbar.ApplyTo(scrollRect.horizontalScrollbar);
			style.VerticalScrollbar.ApplyTo(scrollRect.verticalScrollbar);
		}

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public override bool SetStyle(Style style)
		{
			SetStyleDefaultItem(style);

			SetStyleColors(style);

			SetStyleScrollRect(style);

			style.Collections.MainBackground.ApplyTo(GetComponent<Image>());

			if (IsTable)
			{
				var image = Utilities.GetOrAddComponent<Image>(Container);
				image.sprite = null;
				image.color = DefaultColor;

				var mask_image = Utilities.GetOrAddComponent<Image>(Container.parent);
				mask_image.sprite = null;

				var mask = Utilities.GetOrAddComponent<Mask>(Container.parent);
				mask.showMaskGraphic = false;

				defaultBackgroundColor = style.Table.Background.Color;
			}

			if (ComponentsPool != null)
			{
				ComponentsColoring(true);
			}

			style.ApplyTo(transform.Find("Header"));

			return true;
		}
#endregion

#region Selectable

		/// <summary>
		/// Selectable data.
		/// </summary>
		protected struct SelectableData : IEquatable<SelectableData>
		{
			/// <summary>
			/// Is need to update EventSystem.currentSelectedGameObject?
			/// </summary>
			public bool Update;

			/// <summary>
			/// index of the item with selectable GameObject.
			/// </summary>
			public int Item;

			/// <summary>
			/// Index of the selectable GameObject of the item.
			/// </summary>
			public int SelectableGameObject;

			/// <summary>
			/// Hash function.
			/// </summary>
			/// <returns>A hash code for the current object.</returns>
			public override int GetHashCode()
			{
				return Item ^ SelectableGameObject;
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				if (!(obj is SelectableData))
				{
					return false;
				}

				return Equals((SelectableData)obj);
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="other">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public bool Equals(SelectableData other)
			{
				if (Update != other.Update)
				{
					return false;
				}

				if (Item != other.Item)
				{
					return false;
				}

				return SelectableGameObject == other.SelectableGameObject;
			}

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="data1">First object.</param>
			/// <param name="data2">Second object.</param>
			/// <returns>true if the objects are equal; otherwise, false.</returns>
			public static bool operator ==(SelectableData data1, SelectableData data2)
			{
				return data1.Equals(data2);
			}

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="data1">First object.</param>
			/// <param name="data2">Second object.</param>
			/// <returns>true if the objects not equal; otherwise, false.</returns>
			public static bool operator !=(SelectableData data1, SelectableData data2)
			{
				return !data1.Equals(data2);
			}
		}

		/// <summary>
		/// Selectable data.
		/// </summary>
		protected SelectableData NewSelectableData;

		/// <summary>
		/// Get current selected GameObject.
		/// </summary>
		/// <returns>Selected GameObject.</returns>
		protected GameObject GetCurrentSelectedGameObject()
		{
			var es = EventSystem.current;
			if (es == null)
			{
				return null;
			}

			var go = es.currentSelectedGameObject;
			if (go == null)
			{
				return null;
			}

			if (!go.transform.IsChildOf(Container))
			{
				return null;
			}

			return go;
		}

		/// <summary>
		/// Get item component with selected GameObject.
		/// </summary>
		/// <param name="go">Selected GameObject.</param>
		/// <returns>Item component.</returns>
		protected TComponent SelectedGameObject2Component(GameObject go)
		{
			if (go == null)
			{
				return null;
			}

			var t = go.transform;
			for (int i = 0; i < Items.Count; i++)
			{
				var item_transform = Items[i].transform;
				if (t.IsChildOf(item_transform) && (t.GetInstanceID() != item_transform.GetInstanceID()))
				{
					return Items[i] as TComponent;
				}
			}

			return null;
		}

		/// <summary>
		/// Find index of the next item.
		/// </summary>
		/// <param name="index">Index of the current item with selected GameObject.</param>
		/// <returns>Index of the next item</returns>
		protected virtual int SelectableFindNextObjectIndex(int index)
		{
			for (int i = 0; i < DataSource.Count; i++)
			{
				var prev_index = index - i;
				var next_index = index + i;
				var prev_valid = IsValid(prev_index);
				var next_valid = IsValid(next_index);
				if (!prev_valid && !next_valid)
				{
					return -1;
				}

				if (IsVisible(next_index))
				{
					return next_index;
				}

				if (IsVisible(prev_index))
				{
					return prev_index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Check currently selected GameObject.
		/// </summary>
		/// <param name="position">Scroll position.</param>
		protected void SelectableCheck(Vector2 position)
		{
			SelectableCheck();
		}

		/// <summary>
		/// Select new selectable GameObject if needed.
		/// </summary>
		/// <param name="position">Scroll position.</param>
		protected void SelectableSet(Vector2 position)
		{
			SelectableSet();
		}

		/// <summary>
		/// Check currently selected GameObject.
		/// </summary>
		protected virtual void SelectableCheck()
		{
			var go = GetCurrentSelectedGameObject();
			if (go == null)
			{
				return;
			}

			var component = SelectedGameObject2Component(go);
			if (component == null)
			{
				return;
			}

			if (IsVisible(component.Index))
			{
				return;
			}

			var item_index = SelectableFindNextObjectIndex(component.Index);
			if (!IsValid(item_index))
			{
				return;
			}

			NewSelectableData.Update = true;
			NewSelectableData.Item = item_index;
			NewSelectableData.SelectableGameObject = component.GetSelectableIndex(go);
		}

		/// <summary>
		/// Select new selectable GameObject if needed.
		/// </summary>
		protected virtual void SelectableSet()
		{
			if (!NewSelectableData.Update)
			{
				return;
			}

			var es = EventSystem.current;
			if ((es == null) || es.alreadySelecting)
			{
				return;
			}

			var component = GetItemComponent(NewSelectableData.Item);
			if (component == null)
			{
				return;
			}

			var go = component.GetSelectableObject(NewSelectableData.SelectableGameObject);
			NewSelectableData.Update = false;

			if (go != null)
			{
				es.SetSelectedGameObject(go);
			}
		}
#endregion
	}
}