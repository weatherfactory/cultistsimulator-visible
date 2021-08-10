namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Attributes;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// ScrollBlock. Show list of the string.
	/// </summary>
	[RequireComponent(typeof(EasyLayoutNS.EasyLayout))]
	[RequireComponent(typeof(Mask))]
	public class ScrollBlock : UIBehaviourConditional, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IScrollHandler, IStylable
	{
		/// <summary>
		/// Do nothing.
		/// </summary>
		public static void DoNothing()
		{
		}

		/// <summary>
		/// Default function to get value.
		/// </summary>
		/// <param name="step">Step.</param>
		/// <returns>Value.</returns>
		public static string DefaultValue(int step)
		{
			return "Index: " + step;
		}

		/// <summary>
		/// Default function to check is interactable.
		/// </summary>
		/// <returns>true.</returns>
		protected static bool DefaultInteractable()
		{
			return true;
		}

		/// <summary>
		/// Default function to check is action allowed.
		/// </summary>
		/// <returns>true.</returns>
		protected static bool DefaultAllow()
		{
			return true;
		}

		/// <summary>
		/// Action to increase the value.
		/// </summary>
		public Action Increase = DoNothing;

		/// <summary>
		/// Action to decrease the value.
		/// </summary>
		public Action Decrease = DoNothing;

		/// <summary>
		/// Function to check is increase allowed.
		/// </summary>
		public Func<bool> AllowIncrease = DefaultAllow;

		/// <summary>
		/// Function to check is decrease allowed.
		/// </summary>
		public Func<bool> AllowDecrease = DefaultAllow;

		/// <summary>
		/// Convert index to the displayed string.
		/// </summary>
		public Func<int, string> Value = DefaultValue;

		/// <summary>
		/// Is interactable?
		/// </summary>
		public Func<bool> IsInteractable = DefaultInteractable;

		/// <summary>
		/// Size of the DefaultItem.
		/// </summary>
		[NonSerialized]
		protected Vector2 DefaultItemSize;

		[SerializeField]
		ScrollBlockItem defaultItem;

		/// <summary>
		/// DefaultItem.
		/// </summary>
		public ScrollBlockItem DefaultItem
		{
			get
			{
				return defaultItem;
			}

			set
			{
				if (defaultItem != value)
				{
					defaultItem = value;

					ComponentsPool.DefaultItem = defaultItem;

					UpdateLayout();
					Resize();
				}
			}
		}

		/// <summary>
		/// Layout.
		/// </summary>
		[NonSerialized]
		protected EasyLayoutBridge Layout;

		/// <summary>
		/// Used instances of the DefaultItem.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<ScrollBlockItem> Components = new List<ScrollBlockItem>();

		/// <summary>
		/// Unused instances of the DefaultItem.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<ScrollBlockItem> ComponentsCache = new List<ScrollBlockItem>();

		ListComponentPool<ScrollBlockItem> componentsPool;

		/// <summary>
		/// Components pool.
		/// </summary>
		protected ListComponentPool<ScrollBlockItem> ComponentsPool
		{
			get
			{
				if (componentsPool == null)
				{
					componentsPool = new ListComponentPool<ScrollBlockItem>(DefaultItem, Components, ComponentsCache, transform as RectTransform);
				}

				return componentsPool;
			}
		}

		/// <summary>
		/// Count of the visible items.
		/// </summary>
		public int Count
		{
			get
			{
				return ComponentsPool.Count;
			}
		}

		/// <summary>
		/// Is horizontal scroll?
		/// </summary>
		[SerializeField]
		protected bool IsHorizontal;

		/// <summary>
		/// Scroll sensitivity.
		/// </summary>
		[SerializeField]
		public float ScrollSensitivity = 15f;

		/// <summary>
		/// Layout internal padding.
		/// </summary>
		public float Padding
		{
			get
			{
				return Layout.GetFiller().x;
			}

			set
			{
				var padding = ClampPadding(value);
				Layout.SetFiller(padding, 0f);
			}
		}

		/// <summary>
		/// Animate inertia scroll with unscaled time.
		/// </summary>
		[SerializeField]
		public bool UnscaledTime = true;

		/// <summary>
		/// Auto-scroll to center after scroll/drag.
		/// </summary>
		[SerializeField]
		public bool AlwaysCenter = true;

		/// <summary>
		/// Time to stop.
		/// </summary>
		[SerializeField]
		[EditorConditionBool("AlwaysCenter")]
		public float TimeToStop = 0.5f;

		/// <summary>
		/// Velocity.
		/// </summary>
		[NonSerialized]
		protected float ScrollVelocity;

		/// <summary>
		/// Inertia velocity.
		/// </summary>
		[NonSerialized]
		protected float IntertiaVelocity;

		/// <summary>
		/// Current deceleration rate.
		/// </summary>
		[NonSerialized]
		protected float CurrentDecelerationRate;

		/// <summary>
		/// Inertia distance.
		/// </summary>
		[NonSerialized]
		protected float InertiaDistance;

		/// <summary>
		/// Is drag event occurring?
		/// </summary>
		[NonSerialized]
		protected bool IsDragging;

		/// <summary>
		/// Is scrolling occurring?
		/// </summary>
		[NonSerialized]
		protected bool IsScrolling;

		/// <summary>
		/// Previous scroll value.
		/// </summary>
		[NonSerialized]
		protected float PrevScrollValue;

		/// <summary>
		/// Current scroll value.
		/// </summary>
		[NonSerialized]
		protected float CurrentScrollValue;

		RectTransform rectTransform;

		/// <summary>
		/// Current RectTransformn.
		/// </summary>
		protected RectTransform RectTransform
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = transform as RectTransform;
				}

				return rectTransform;
			}
		}

		/// <summary>
		/// Median index of the components.
		/// </summary>
		protected int ComponentsMedian
		{
			get
			{
				return ComponentsPool.Count / 2;
			}
		}

		/// <summary>
		/// Distance to center.
		/// </summary>
		public float DistanceToCenter
		{
			get
			{
				return GetCenter() - Padding;
			}
		}

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected override void Start()
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

			UpdateLayout();

			var resizer = Utilities.GetOrAddComponent<ResizeListener>(this);
			resizer.OnResize.AddListener(Resize);

			Resize();

			AlignComponents();
		}

		/// <summary>
		/// Clamp padding value.
		/// </summary>
		/// <param name="padding">Padding.</param>
		/// <returns>Clamped value.</returns>
		protected float ClampPadding(float padding)
		{
			var current = Layout.GetFiller().x;
			var delta = current - padding;
			if ((delta < 0) && (!AllowDecrease()))
			{
				return current;
			}

			if ((delta > 0) && (!AllowIncrease()))
			{
				return current;
			}

			var size = ItemFullSize();
			var center = GetCenter();
			if (Mathf.Round(padding) > (-size))
			{
				var n = Mathf.FloorToInt((Mathf.Round(padding) - center) / size);
				if (n > 0)
				{
					for (int i = 0; i < n; i++)
					{
						if (!AllowDecrease())
						{
							break;
						}

						padding -= size;
						Decrease();
					}

					UpdateView();
				}
			}
			else if (Mathf.Round(padding) < (-size))
			{
				var n = Mathf.FloorToInt((Mathf.Round(-padding) + center) / size);
				if (n > 0)
				{
					for (int i = 0; i < n; i++)
					{
						if (!AllowIncrease())
						{
							break;
						}

						padding += size;
						Increase();
					}

					UpdateView();
				}
			}

			return padding;
		}

		/// <summary>
		/// Update the layout.
		/// </summary>
		protected void UpdateLayout()
		{
			Layout = new EasyLayoutBridge(GetComponent<EasyLayoutNS.EasyLayout>(), DefaultItem.transform as RectTransform, false, false)
			{
				IsHorizontal = IsHorizontal,
			};

			DefaultItemSize = Layout.GetItemSize();
			DefaultItem.gameObject.SetActive(false);
		}

		/// <summary>
		/// Container size.
		/// </summary>
		/// <returns>Size.</returns>
		protected float ContainerSize()
		{
			return (IsHorizontal ? RectTransform.rect.width : RectTransform.rect.height) - Layout.GetFullMargin();
		}

		/// <summary>
		/// Content size.
		/// </summary>
		/// <returns>Size.</returns>
		protected float ContentSize()
		{
			return (ItemFullSize() * ComponentsPool.Count) - Layout.GetSpacing();
		}

		/// <summary>
		/// Item size.
		/// </summary>
		/// <returns>Size.</returns>
		protected float ItemSize()
		{
			return IsHorizontal ? DefaultItemSize.x : DefaultItemSize.y;
		}

		/// <summary>
		/// Item size with spacing.
		/// </summary>
		/// <returns>Size.</returns>
		protected float ItemFullSize()
		{
			return ItemSize() + Layout.GetSpacing();
		}

		/// <summary>
		/// Calculate the maximum count of the visible components.
		/// </summary>
		/// <returns>Maximum count of the visible components.</returns>
		protected int CalculateMax()
		{
			var result = Mathf.CeilToInt((ContainerSize() + Layout.GetSpacing()) / ItemFullSize()) + 1;
			if (result < 0)
			{
				result = 0;
			}

			if ((result % 2) == 0)
			{
				result += 1;
			}

			return result;
		}

		/// <summary>
		/// Is component is null?
		/// </summary>
		/// <param name="component">Component.</param>
		/// <returns>true if component is null; otherwise, false.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Reviewed.")]
		protected virtual bool IsNullComponent(ScrollBlockItem component)
		{
			return component == null;
		}

		/// <summary>
		/// Process RectTransform resize.
		/// </summary>
		protected void Resize()
		{
			var max = CalculateMax();

			if (max == ComponentsPool.Count)
			{
				return;
			}

			ComponentsPool.Require(max);

			var median = ComponentsMedian;
			for (int i = 0; i < ComponentsPool.Count; i++)
			{
				ComponentsPool[i].Index = i - median;
				ComponentsPool[i].Owner = this;
				ComponentsPool[i].transform.SetAsLastSibling();
			}

			UpdateView();
		}

		/// <summary>
		/// Set text of the specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected void SetComponentText(ScrollBlockItem component)
		{
			component.Text.Value = Value(component.Index);
		}

		/// <summary>
		/// Set text.
		/// </summary>
		[Obsolete("Replaced with UpdateView().")]
		public void SetText()
		{
			UpdateView();
		}

		/// <summary>
		/// Update view.
		/// </summary>
		public void UpdateView()
		{
			Init();
			ComponentsPool.ForEach(SetComponentText);
		}

		/// <summary>
		/// Returns true if the GameObject and the Component are active.
		/// </summary>
		/// <returns>true if the GameObject and the Component are active; otherwise false.</returns>
		public override bool IsActive()
		{
			return base.IsActive() && isInited && IsInteractable();
		}

		/// <summary>
		/// Process the begin drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			if (!IsActive())
			{
				return;
			}

			IsDragging = true;

			PrevScrollValue = Padding;
			CurrentScrollValue = Padding;

			StopInertia();
		}

		/// <summary>
		/// Process the drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnDrag(PointerEventData eventData)
		{
			if (!IsDragging)
			{
				return;
			}

			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			if (!IsActive())
			{
				return;
			}

			StopInertia();
			var scroll_delta = IsHorizontal ? eventData.delta.x : -eventData.delta.y;
			Scroll(scroll_delta);
		}

		/// <summary>
		/// Process scroll event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnScroll(PointerEventData eventData)
		{
			if (!IsActive())
			{
				return;
			}

			IsScrolling = true;
			var scroll_delta = IsHorizontal ? eventData.scrollDelta.x : -eventData.scrollDelta.y;
			Scroll(scroll_delta * ScrollSensitivity);
		}

		/// <summary>
		/// Scroll.
		/// </summary>
		/// <param name="delta">Delta.</param>
		protected virtual void Scroll(float delta)
		{
			Padding += delta;

			CurrentScrollValue += delta;
			var time_delta = UtilitiesTime.DefaultGetDeltaTime(UnscaledTime);
			var new_velocity = (PrevScrollValue - CurrentScrollValue) / time_delta;
			ScrollVelocity = Mathf.Lerp(ScrollVelocity, new_velocity, time_delta * 10);
			PrevScrollValue = CurrentScrollValue;
		}

		/// <summary>
		/// Process the end drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (!IsDragging)
			{
				return;
			}

			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			IsDragging = false;
			InitIntertia();
		}

		/// <summary>
		/// Init inertia.
		/// </summary>
		protected virtual void InitIntertia()
		{
			IntertiaVelocity = -ScrollVelocity;
			CurrentDecelerationRate = -IntertiaVelocity / TimeToStop;

			var direction = Mathf.Sign(IntertiaVelocity);
			var time_to_stop_sq = Mathf.Pow(TimeToStop, 2f);
			var distance = ((-Mathf.Abs(CurrentDecelerationRate) * time_to_stop_sq) / 2f) + (Mathf.Abs(IntertiaVelocity) * TimeToStop);
			InertiaDistance = ClampDistance(distance, direction);
			IntertiaVelocity = (InertiaDistance - (-Mathf.Abs(CurrentDecelerationRate) * (TimeToStop * TimeToStop) / 2f)) / TimeToStop;
			IntertiaVelocity *= direction;
		}

		/// <summary>
		/// Late update.
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (IsScrolling)
			{
				IsScrolling = false;
				InitIntertia();
			}
			else if (!IsDragging && (InertiaDistance > 0f) && AlwaysCenter)
			{
				var delta = UtilitiesTime.DefaultGetDeltaTime(UnscaledTime);
				var distance = IntertiaVelocity > 0f
					? Mathf.Min(InertiaDistance, IntertiaVelocity * delta)
					: Mathf.Max(-InertiaDistance, IntertiaVelocity * delta);

				Padding += distance;
				InertiaDistance -= Mathf.Abs(distance);

				if (InertiaDistance > 0f)
				{
					IntertiaVelocity += CurrentDecelerationRate * delta;
					ScrollVelocity = -IntertiaVelocity;
				}
				else
				{
					StopInertia();
				}
			}
		}

		/// <summary>
		/// Stop inertia.
		/// </summary>
		protected void StopInertia()
		{
			CurrentDecelerationRate = 0f;
			InertiaDistance = 0f;
		}

		/// <summary>
		/// Clamp distance to stop right at value.
		/// </summary>
		/// <param name="distance">Distance.</param>
		/// <param name="direction">Scroll direction.</param>
		/// <returns>Clamped distance.</returns>
		protected float ClampDistance(float distance, float direction)
		{
			var extra = (GetCenter() - Padding) * direction;
			var steps = Mathf.Round((Mathf.Abs(distance) - extra) / ItemFullSize());
			var new_distance = (steps * ItemFullSize()) + extra;
			return new_distance;
		}

		/// <summary>
		/// Get center.
		/// </summary>
		/// <returns>Center.</returns>
		protected float GetCenter()
		{
			return -(ContentSize() - ContainerSize()) / 2f;
		}

		/// <summary>
		/// Align components.
		/// </summary>
		protected void AlignComponents()
		{
			Padding = GetCenter();
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected override void OnDestroy()
		{
			var resizer = GetComponent<ResizeListener>();
			if (resizer != null)
			{
				resizer.OnResize.RemoveListener(Resize);
			}
		}

		/// <summary>
		/// Called by a BaseInputModule when a drag has been found but before it is valid to begin the drag.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
		}

		#region IStylable implementation

		/// <inheritdoc/>
		public bool SetStyle(Style style)
		{
			if (DefaultItem.Text != null)
			{
				style.ScrollBlock.Text.ApplyTo(DefaultItem.Text.GameObject);

				if (isInited)
				{
					for (int i = 0; i < Components.Count; i++)
					{
						style.ScrollBlock.Text.ApplyTo(Components[i].Text.GameObject);
					}

					for (int i = 0; i < ComponentsCache.Count; i++)
					{
						style.ScrollBlock.Text.ApplyTo(ComponentsCache[i].Text.GameObject);
					}
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public bool GetStyle(Style style)
		{
			if (DefaultItem.Text != null)
			{
				style.ScrollBlock.Text.GetFrom(DefaultItem.Text.GameObject);
			}

			return true;
		}
		#endregion
	}
}