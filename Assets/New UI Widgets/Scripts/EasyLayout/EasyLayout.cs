namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// EasyLayout.
	/// Warning: using RectTransform relative size with positive size delta (like 100% + 10) with ContentSizeFitter can lead to infinite increased size.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/New UI Widgets/Layout/EasyLayout")]
	public class EasyLayout : LayoutGroup, INotifyPropertyChanged
	{
		readonly List<LayoutElementInfo> elements = new List<LayoutElementInfo>();

		readonly Stack<LayoutElementInfo> elementsCache = new Stack<LayoutElementInfo>();

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = DefaultPropertyHandler;

		/// <summary>
		/// Occurs when a properties values changed except PaddingInner property.
		/// </summary>
		[SerializeField]
		public UnityEvent SettingsChanged = new UnityEvent();

		[SerializeField]
		[FormerlySerializedAs("GroupPosition")]
		Anchors groupPosition = Anchors.UpperLeft;

		/// <summary>
		/// The group position.
		/// </summary>
		public Anchors GroupPosition
		{
			get
			{
				return groupPosition;
			}

			set
			{
				if (groupPosition != value)
				{
					groupPosition = value;
					Changed("GroupPosition");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("Stacking")]
		Axis mainAxis = Axis.Horizontal;

		/// <summary>
		/// The stacking type.
		/// </summary>
		[Obsolete("Replaced with MainAxis.")]
		public Stackings Stacking
		{
			get
			{
				if (MainAxis == Axis.Horizontal)
				{
					return Stackings.Horizontal;
				}

				return Stackings.Vertical;
			}

			set
			{
				MainAxis = (value == Stackings.Horizontal) ? Axis.Horizontal : Axis.Vertical;
			}
		}

		/// <summary>
		/// Main axis.
		/// </summary>
		public Axis MainAxis
		{
			get
			{
				return mainAxis;
			}

			set
			{
				if (mainAxis != value)
				{
					mainAxis = value;
					Changed("Axis");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("LayoutType")]
		LayoutTypes layoutType = LayoutTypes.Compact;

		/// <summary>
		/// The type of the layout.
		/// </summary>
		public LayoutTypes LayoutType
		{
			get
			{
				return layoutType;
			}

			set
			{
				if (layoutType != value)
				{
					layoutGroup = null;
					layoutType = value;
					Changed("LayoutType");
				}
			}
		}

		EasyLayoutBaseType layoutGroup;

		/// <summary>
		/// Layout group.
		/// </summary>
		protected EasyLayoutBaseType LayoutGroup
		{
			get
			{
				if (layoutGroup == null)
				{
					layoutGroup = GetLayoutGroup();
				}

				return layoutGroup;
			}
		}

		[SerializeField]
		[FormerlySerializedAs("CompactConstraint")]
		CompactConstraints compactConstraint = CompactConstraints.Flexible;

		/// <summary>
		/// Which constraint to use for the Grid layout.
		/// </summary>
		public CompactConstraints CompactConstraint
		{
			get
			{
				return compactConstraint;
			}

			set
			{
				if (compactConstraint != value)
				{
					compactConstraint = value;
					Changed("CompactConstraint");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("CompactConstraintCount")]
		int compactConstraintCount = 1;

		/// <summary>
		/// How many elements there should be along the constrained axis.
		/// </summary>
		public int CompactConstraintCount
		{
			get
			{
				return Mathf.Max(1, compactConstraintCount);
			}

			set
			{
				if (compactConstraintCount != value)
				{
					compactConstraintCount = value;
					Changed("CompactConstraintCount");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("GridConstraint")]
		GridConstraints gridConstraint = GridConstraints.Flexible;

		/// <summary>
		/// Which constraint to use for the Grid layout.
		/// </summary>
		public GridConstraints GridConstraint
		{
			get
			{
				return gridConstraint;
			}

			set
			{
				if (gridConstraint != value)
				{
					gridConstraint = value;
					Changed("GridConstraint");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("GridConstraintCount")]
		int gridConstraintCount = 1;

		/// <summary>
		/// How many cells there should be along the constrained axis.
		/// </summary>
		public int GridConstraintCount
		{
			get
			{
				return Mathf.Max(1, gridConstraintCount);
			}

			set
			{
				if (gridConstraintCount != value)
				{
					gridConstraintCount = value;
					Changed("GridConstraintCount");
				}
			}
		}

		/// <summary>
		/// Constraint count.
		/// </summary>
		public int ConstraintCount
		{
			get
			{
				if (LayoutType == LayoutTypes.Compact)
				{
					return CompactConstraintCount;
				}
				else
				{
					return GridConstraintCount;
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("RowAlign")]
		HorizontalAligns rowAlign = HorizontalAligns.Left;

		/// <summary>
		/// The row align.
		/// </summary>
		public HorizontalAligns RowAlign
		{
			get
			{
				return rowAlign;
			}

			set
			{
				if (rowAlign != value)
				{
					rowAlign = value;
					Changed("RowAlign");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("InnerAlign")]
		InnerAligns innerAlign = InnerAligns.Top;

		/// <summary>
		/// The inner align.
		/// </summary>
		public InnerAligns InnerAlign
		{
			get
			{
				return innerAlign;
			}

			set
			{
				if (innerAlign != value)
				{
					innerAlign = value;
					Changed("InnerAlign");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("CellAlign")]
		Anchors cellAlign = Anchors.UpperLeft;

		/// <summary>
		/// The cell align.
		/// </summary>
		public Anchors CellAlign
		{
			get
			{
				return cellAlign;
			}

			set
			{
				if (cellAlign != value)
				{
					cellAlign = value;
					Changed("CellAlign");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("Spacing")]
		Vector2 spacing = new Vector2(5, 5);

		/// <summary>
		/// The spacing.
		/// </summary>
		public Vector2 Spacing
		{
			get
			{
				return spacing;
			}

			set
			{
				if (spacing != value)
				{
					spacing = value;
					Changed("Spacing");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("Symmetric")]
		bool symmetric = true;

		/// <summary>
		/// Symmetric margin.
		/// </summary>
		public bool Symmetric
		{
			get
			{
				return symmetric;
			}

			set
			{
				if (symmetric != value)
				{
					symmetric = value;
					Changed("Symmetric");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("Margin")]
		Vector2 margin = new Vector2(5, 5);

		/// <summary>
		/// The margin.
		/// </summary>
		public Vector2 Margin
		{
			get
			{
				return margin;
			}

			set
			{
				if (margin != value)
				{
					margin = value;
					Changed("Margin");
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		Padding marginInner;

		/// <summary>
		/// The margin.
		/// Should be used by ListView related scripts.
		/// </summary>
		public Padding MarginInner
		{
			get
			{
				return marginInner;
			}

			set
			{
				if (marginInner != value)
				{
					marginInner = value;
					Changed("MarginInner");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("PaddingInner")]
		[HideInInspector]
		Padding paddingInner;

		/// <summary>
		/// The padding.
		/// Should be used by ListView related scripts.
		/// </summary>
		public Padding PaddingInner
		{
			get
			{
				return paddingInner;
			}

			set
			{
				if (paddingInner != value)
				{
					paddingInner = value;
					Changed("PaddingInner", false);
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("MarginTop")]
		float marginTop = 5f;

		/// <summary>
		/// The margin top.
		/// </summary>
		public float MarginTop
		{
			get
			{
				return marginTop;
			}

			set
			{
				if (marginTop != value)
				{
					marginTop = value;
					Changed("MarginTop");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("MarginBottom")]
		float marginBottom = 5f;

		/// <summary>
		/// The margin bottom.
		/// </summary>
		public float MarginBottom
		{
			get
			{
				return marginBottom;
			}

			set
			{
				if (marginBottom != value)
				{
					marginBottom = value;
					Changed("MarginBottom");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("MarginLeft")]
		float marginLeft = 5f;

		/// <summary>
		/// The margin left.
		/// </summary>
		public float MarginLeft
		{
			get
			{
				return marginLeft;
			}

			set
			{
				if (marginLeft != value)
				{
					marginLeft = value;
					Changed("MarginLeft");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("MarginRight")]
		float marginRight = 5f;

		/// <summary>
		/// The margin right.
		/// </summary>
		public float MarginRight
		{
			get
			{
				return marginRight;
			}

			set
			{
				if (marginRight != value)
				{
					marginRight = value;
					Changed("MarginRight");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("RightToLeft")]
		bool rightToLeft = false;

		/// <summary>
		/// The right to left stacking.
		/// </summary>
		public bool RightToLeft
		{
			get
			{
				return rightToLeft;
			}

			set
			{
				if (rightToLeft != value)
				{
					rightToLeft = value;
					Changed("RightToLeft");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("TopToBottom")]
		bool topToBottom = true;

		/// <summary>
		/// The top to bottom stacking.
		/// </summary>
		public bool TopToBottom
		{
			get
			{
				return topToBottom;
			}

			set
			{
				if (topToBottom != value)
				{
					topToBottom = value;
					Changed("TopToBottom");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("SkipInactive")]
		bool skipInactive = true;

		/// <summary>
		/// The skip inactive.
		/// </summary>
		public bool SkipInactive
		{
			get
			{
				return skipInactive;
			}

			set
			{
				if (skipInactive != value)
				{
					skipInactive = value;
					Changed("SkipInactive");
				}
			}
		}

		[SerializeField]
		bool resetRotation;

		/// <summary>
		/// Reset rotation for the controlled elements.
		/// </summary>
		public bool ResetRotation
		{
			get
			{
				return resetRotation;
			}

			set
			{
				if (resetRotation != value)
				{
					resetRotation = value;
					Changed("ResetRotation");
				}
			}
		}

		Func<IEnumerable<GameObject>, IEnumerable<GameObject>> filter;

		/// <summary>
		/// The filter.
		/// </summary>
		public Func<IEnumerable<GameObject>, IEnumerable<GameObject>> Filter
		{
			get
			{
				return filter;
			}

			set
			{
				if (filter != value)
				{
					filter = value;
					Changed("Filter");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("ChildrenWidth")]
		ChildrenSize childrenWidth;

		/// <summary>
		/// How to control width of the children.
		/// </summary>
		public ChildrenSize ChildrenWidth
		{
			get
			{
				return childrenWidth;
			}

			set
			{
				if (childrenWidth != value)
				{
					childrenWidth = value;
					Changed("ChildrenWidth");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("ChildrenHeight")]
		ChildrenSize childrenHeight;

		/// <summary>
		/// How to control height of the children.
		/// </summary>
		public ChildrenSize ChildrenHeight
		{
			get
			{
				return childrenHeight;
			}

			set
			{
				if (childrenHeight != value)
				{
					childrenHeight = value;
					Changed("ChildrenHeight");
				}
			}
		}

		[SerializeField]
		EasyLayoutFlexSettings flexSettings = new EasyLayoutFlexSettings();

		/// <summary>
		/// Settings for the Flex layout type.
		/// </summary>
		public EasyLayoutFlexSettings FlexSettings
		{
			get
			{
				return flexSettings;
			}

			set
			{
				if (flexSettings != value)
				{
					flexSettings.PropertyChanged -= FlexSettingsChanged;
					flexSettings = value;
					flexSettings.PropertyChanged += FlexSettingsChanged;
					Changed("FlexSettings");
				}
			}
		}

		[SerializeField]
		EasyLayoutStaggeredSettings staggeredSettings = new EasyLayoutStaggeredSettings();

		/// <summary>
		/// Settings for the Staggered layout type.
		/// </summary>
		public EasyLayoutStaggeredSettings StaggeredSettings
		{
			get
			{
				return staggeredSettings;
			}

			set
			{
				if (staggeredSettings != value)
				{
					staggeredSettings.PropertyChanged -= StaggeredSettingsChanged;
					staggeredSettings = value;
					staggeredSettings.PropertyChanged += StaggeredSettingsChanged;
					Changed("StaggeredSettings");
				}
			}
		}

		[SerializeField]
		EasyLayoutEllipseSettings ellipseSettings = new EasyLayoutEllipseSettings();

		/// <summary>
		/// Settings for the Ellipse layout type.
		/// </summary>
		public EasyLayoutEllipseSettings EllipseSettings
		{
			get
			{
				return ellipseSettings;
			}

			set
			{
				if (ellipseSettings != value)
				{
					ellipseSettings.PropertyChanged -= EllipseSettingsChanged;
					ellipseSettings = value;
					ellipseSettings.PropertyChanged += EllipseSettingsChanged;
					Changed("EllipseSettings");
				}
			}
		}

		/// <summary>
		/// Control width of children.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Use ChildrenWidth with ChildrenSize.SetPreferred instead.")]
		public bool ControlWidth;

		/// <summary>
		/// Control height of children.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Use ChildrenHeight with ChildrenSize.SetPreferred instead.")]
		[FormerlySerializedAs("ControlHeight")]
		public bool ControlHeight;

		/// <summary>
		/// Sets width of the children to maximum width from them.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Use ChildrenWidth with ChildrenSize.SetMaxFromPreferred instead.")]
		[FormerlySerializedAs("MaxWidth")]
		public bool MaxWidth;

		/// <summary>
		/// Sets height of the children to maximum height from them.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Use ChildrenHeight with ChildrenSize.SetMaxFromPreferred instead.")]
		[FormerlySerializedAs("MaxHeight")]
		public bool MaxHeight;

		/// <summary>
		/// Internal size.
		/// </summary>
		public Vector2 InternalSize
		{
			get
			{
				var size = rectTransform.rect.size;
				size.x -= MarginFullHorizontal + PaddingInner.Horizontal;
				size.y -= MarginFullVertical + PaddingInner.Vertical;

				return size;
			}
		}

		/// <summary>
		/// Gets or sets the size of the inner block.
		/// </summary>
		/// <value>The size of the inner block.</value>
		public Vector2 BlockSize
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the UI size.
		/// </summary>
		/// <value>The UI size.</value>
		public Vector2 UISize
		{
			get;
			protected set;
		}

		/// <summary>
		/// Size in elements.
		/// </summary>
		public Vector2 Size
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the minimum height.
		/// </summary>
		/// <value>The minimum height.</value>
		public override float minHeight
		{
			get
			{
				return BlockSize[1];
			}
		}

		/// <summary>
		/// Gets the minimum width.
		/// </summary>
		/// <value>The minimum width.</value>
		public override float minWidth
		{
			get
			{
				return BlockSize[0];
			}
		}

		/// <summary>
		/// Gets the preferred height.
		/// </summary>
		/// <value>The preferred height.</value>
		public override float preferredHeight
		{
			get
			{
				return BlockSize[1];
			}
		}

		/// <summary>
		/// Gets the preferred width.
		/// </summary>
		/// <value>The preferred width.</value>
		public override float preferredWidth
		{
			get
			{
				return BlockSize[0];
			}
		}

		/// <summary>
		/// Summary horizontal margin.
		/// </summary>
		public float MarginHorizontal
		{
			get
			{
				return Symmetric ? (Margin.x + Margin.x) : (MarginLeft + MarginRight);
			}
		}

		/// <summary>
		/// Summary vertical margin.
		/// </summary>
		public float MarginVertical
		{
			get
			{
				return Symmetric ? (Margin.y + Margin.y) : (MarginTop + MarginBottom);
			}
		}

		/// <summary>
		/// Summary horizontal margin with MarginInner.
		/// </summary>
		public float MarginFullHorizontal
		{
			get
			{
				var margin_external = Symmetric ? (Margin.x + Margin.x) : (MarginLeft + MarginRight);
				return margin_external + MarginInner.Horizontal;
			}
		}

		/// <summary>
		/// Summary vertical margin with MarginInner.
		/// </summary>
		public float MarginFullVertical
		{
			get
			{
				var margin_external = Symmetric ? (Margin.y + Margin.y) : (MarginTop + MarginBottom);
				return margin_external + MarginInner.Vertical;
			}
		}

		/// <summary>
		/// Is horizontal stacking?
		/// </summary>
		public bool IsHorizontal
		{
			get
			{
				return MainAxis == Axis.Horizontal;
			}
		}

		/// <summary>
		/// Size of the main axis.
		/// </summary>
		public float MainAxisSize
		{
			get
			{
				return IsHorizontal
					? rectTransform.rect.width - MarginFullHorizontal
					: rectTransform.rect.height - MarginFullVertical;
			}
		}

		/// <summary>
		/// Size of the sub axis.
		/// </summary>
		public float SubAxisSize
		{
			get
			{
				return !IsHorizontal
					? rectTransform.rect.width - MarginFullHorizontal
					: rectTransform.rect.height - MarginFullVertical;
			}
		}

		/// <summary>
		/// Properties tracker.
		/// </summary>
		protected DrivenRectTransformTracker PropertiesTracker;

		/// <summary>
		/// Property changed.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		/// <param name="invokeSettingsChanged">Should invoke SettingsChanged event?</param>
		protected void Changed(string propertyName, bool invokeSettingsChanged = true)
		{
			SetDirty();

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

			if (invokeSettingsChanged)
			{
				SettingsChanged.Invoke();
			}
		}

		void FlexSettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			Changed("FlexSettings");
		}

		void StaggeredSettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			Changed("StaggeredSettings");
		}

		void EllipseSettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			Changed("EllipseSettings");
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected override void Start()
		{
			flexSettings.PropertyChanged += FlexSettingsChanged;
			staggeredSettings.PropertyChanged += StaggeredSettingsChanged;
			ellipseSettings.PropertyChanged += EllipseSettingsChanged;
		}

		/// <summary>
		/// Process the disable event.
		/// </summary>
		protected override void OnDisable()
		{
			PropertiesTracker.Clear();

			base.OnDisable();
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected override void OnDestroy()
		{
			if (flexSettings != null)
			{
				flexSettings.PropertyChanged -= FlexSettingsChanged;
			}

			if (ellipseSettings != null)
			{
				ellipseSettings.PropertyChanged -= EllipseSettingsChanged;
			}

			if (staggeredSettings != null)
			{
				staggeredSettings.PropertyChanged -= StaggeredSettingsChanged;
			}

			base.OnDestroy();
		}

		/// <summary>
		/// Process the RectTransform removed event.
		/// </summary>
		protected virtual void OnRectTransformRemoved()
		{
			SetDirty();
		}

		/// <summary>
		/// Sets the layout horizontal.
		/// </summary>
		public override void SetLayoutHorizontal()
		{
			RepositionElements();
		}

		/// <summary>
		/// Sets the layout vertical.
		/// </summary>
		public override void SetLayoutVertical()
		{
			RepositionElements();
		}

		/// <summary>
		/// Calculates the layout input horizontal.
		/// </summary>
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalculateLayoutSize();
		}

		/// <summary>
		/// Calculates the layout input vertical.
		/// </summary>
		public override void CalculateLayoutInputVertical()
		{
			CalculateLayoutSize();
		}

		/// <summary>
		/// Marks layout to update.
		/// </summary>
		public void NeedUpdateLayout()
		{
			SetDirty();
		}

		/// <summary>
		/// Calculates the size of the layout.
		/// </summary>
		public void CalculateLayoutSize()
		{
			UpdateElements();

			PerformLayout(false);
		}

		/// <summary>
		/// Repositions the user interface elements.
		/// </summary>
		void RepositionElements()
		{
			UpdateElements();

			PerformLayout(true);
		}

		/// <summary>
		/// Updates the layout.
		/// </summary>
		public void UpdateLayout()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		/// <summary>
		/// Is IgnoreLayout enabled?
		/// </summary>
		/// <param name="rect">RectTransform</param>
		/// <returns>true if IgnoreLayout enabled; otherwise, false.</returns>
		protected static bool IsIgnoreLayout(Transform rect)
		{
			#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
			var ignorer = rect.GetComponent<ILayoutIgnorer>();
			#else
			var ignorer = rect.GetComponent(typeof(ILayoutIgnorer)) as ILayoutIgnorer;
			#endif
			return (ignorer != null) && ignorer.ignoreLayout;
		}

		/// <summary>
		/// Get children.
		/// </summary>
		/// <returns>Children.</returns>
		protected List<RectTransform> GetChildren()
		{
			var children = rectChildren;

			if (!SkipInactive)
			{
				children = new List<RectTransform>();
				foreach (Transform child in transform)
				{
					if (!IsIgnoreLayout(child))
					{
						children.Add(child as RectTransform);
					}
				}
			}

			if (Filter != null)
			{
				children = ApplyFilter(children);
			}

			return children;
		}

		/// <summary>
		/// Update LayoutElements.
		/// </summary>
		protected void UpdateElements()
		{
			ClearElements();

			var children = GetChildren();
			for (int i = 0; i < children.Count; i++)
			{
				elements.Add(CreateElement(children[i]));
			}
		}

		/// <summary>
		/// Reset layout elements list.
		/// </summary>
		protected void ClearElements()
		{
			for (int i = 0; i < elements.Count; i++)
			{
				elementsCache.Push(elements[i]);
			}

			elements.Clear();
		}

		/// <summary>
		/// Create layout element.
		/// </summary>
		/// <param name="elem">Element.</param>
		/// <returns>Element data.</returns>
		protected LayoutElementInfo CreateElement(RectTransform elem)
		{
			var info = (elementsCache.Count > 0) ? elementsCache.Pop() : new LayoutElementInfo();
			var active = SkipInactive || elem.gameObject.activeInHierarchy;
			info.SetElement(elem, active, this);
			if (ResetRotation && (LayoutType != LayoutTypes.Ellipse))
			{
				info.NewEulerAngles = Vector3.zero;
			}

			return info;
		}

		/// <summary>
		/// Apply filter.
		/// </summary>
		/// <param name="input">Original list.</param>
		/// <returns>Filtered list.</returns>
		protected List<RectTransform> ApplyFilter(List<RectTransform> input)
		{
			var objects = new GameObject[input.Count];

			for (int i = 0; i < input.Count; i++)
			{
				objects[i] = input[i].gameObject;
			}

			var result = new List<RectTransform>();
			foreach (var elem in Filter(objects))
			{
				result.Add(elem.transform as RectTransform);
			}

			return result;
		}

		/// <summary>
		/// Gets the margin top.
		/// </summary>
		/// <returns>Top margin.</returns>
		public float GetMarginTop()
		{
			return Symmetric ? Margin.y : MarginTop;
		}

		/// <summary>
		/// Gets the margin bottom.
		/// </summary>
		/// <returns>Bottom margin.</returns>
		public float GetMarginBottom()
		{
			return Symmetric ? Margin.y : MarginBottom;
		}

		/// <summary>
		/// Gets the margin left.
		/// </summary>
		/// <returns>Left margin.</returns>
		public float GetMarginLeft()
		{
			return Symmetric ? Margin.x : MarginLeft;
		}

		/// <summary>
		/// Gets the margin right.
		/// </summary>
		/// <returns>Right margin.</returns>
		public float GetMarginRight()
		{
			return Symmetric ? Margin.x : MarginRight;
		}

		/// <summary>
		/// Get layout group.
		/// </summary>
		/// <returns>Layout group.</returns>
		protected EasyLayoutBaseType GetLayoutGroup()
		{
			switch (LayoutType)
			{
				case LayoutTypes.Compact:
					return new EasyLayoutCompact(this);
				case LayoutTypes.Grid:
					return new EasyLayoutGrid(this);
				case LayoutTypes.Flex:
					return new EasyLayoutFlex(this);
				case LayoutTypes.Staggered:
					return new EasyLayoutStaggered(this);
				case LayoutTypes.Ellipse:
					return new EasyLayoutEllipse(this);
				default:
					Debug.LogWarning("Unknown layout type: " + LayoutType);
					break;
			}

			return null;
		}

		/// <summary>
		/// Perform layout.
		/// </summary>
		/// <param name="setPositions">Is need to set elements position?</param>
		protected void PerformLayout(bool setPositions)
		{
			if (LayoutGroup == null)
			{
				Debug.LogWarning("Layout group not found: " + LayoutType);
				return;
			}

			PropertiesTracker.Clear();
			var size = LayoutGroup.PerformLayout(elements, setPositions);

			UISize = elements.Count > 0 ? new Vector2(size.x, size.y) : Vector2.zero;
			BlockSize = new Vector2(UISize.x + MarginFullHorizontal, UISize.y + MarginFullVertical);
		}

		/// <summary>
		/// Set element size.
		/// </summary>
		/// <param name="element">Element.</param>
		public void SetElementSize(LayoutElementInfo element)
		{
			var driven_properties = DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.AnchoredPositionZ;

			if (ChildrenWidth != ChildrenSize.DoNothing)
			{
				driven_properties |= DrivenTransformProperties.SizeDeltaX;
				element.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, element.NewWidth);
			}

			if (ChildrenHeight != ChildrenSize.DoNothing)
			{
				driven_properties |= DrivenTransformProperties.SizeDeltaY;
				element.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, element.NewHeight);
			}

			if (LayoutType == LayoutTypes.Ellipse || ResetRotation)
			{
				driven_properties |= DrivenTransformProperties.Rotation;
				element.Rect.localEulerAngles = element.NewEulerAngles;
			}

			if (LayoutType == LayoutTypes.Ellipse)
			{
				driven_properties |= DrivenTransformProperties.Pivot;
				element.Rect.pivot = element.NewPivot;
			}

			PropertiesTracker.Add(this, element.Rect, driven_properties);
		}

		/// <summary>
		/// Get element position in the group.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>Position.</returns>
		public EasyLayoutPosition GetElementPosition(RectTransform element)
		{
			return LayoutGroup.GetElementPosition(element);
		}

		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			Upgrade();
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Update layout when parameters changed.
		/// </summary>
		protected override void OnValidate()
		{
			layoutGroup = null;

			SetDirty();
		}
		#endif

		[SerializeField]
		[HideInInspector]
		int version = 0;

		#pragma warning disable 0618
		/// <summary>
		/// Upgrade to keep compatibility between versions.
		/// </summary>
		public virtual void Upgrade()
		{
			// upgrade to 1.6
			if (version == 0)
			{
				if (ControlWidth)
				{
					ChildrenWidth = MaxWidth ? ChildrenSize.SetMaxFromPreferred : ChildrenSize.SetPreferred;
				}

				if (ControlHeight)
				{
					ChildrenHeight = MaxHeight ? ChildrenSize.SetMaxFromPreferred : ChildrenSize.SetPreferred;
				}
			}

			version = 1;
		}
		#pragma warning restore 0618

		/// <summary>
		/// Default property handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public static void DefaultPropertyHandler(object sender, PropertyChangedEventArgs e)
		{
		}
	}
}