namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.Serialization;

	/// <summary>
	/// Drag support.
	/// Drop component should implement IDropSupport{T} with same type.
	/// </summary>
	/// <typeparam name="T">Type of draggable data.</typeparam>
	public abstract class DragSupport<T> : BaseDragSupport, ICancelHandler
	{
		/// <summary>
		/// Allow drag.
		/// </summary>
		[SerializeField]
		public bool AllowDrag = true;

		/// <summary>
		/// Gets or sets the data.
		/// Data will be passed to Drop component.
		/// </summary>
		/// <value>The data.</value>
		public T Data
		{
			get;
			protected set;
		}

		/// <summary>
		/// The Allow drop cursor texture.
		/// </summary>
		[SerializeField]
		public Texture2D AllowDropCursor;

		/// <summary>
		/// The Allow drop cursor hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 AllowDropCursorHotSpot = new Vector2(4, 2);

		/// <summary>
		/// The Denied drop cursor texture.
		/// </summary>
		[SerializeField]
		public Texture2D DeniedDropCursor;

		/// <summary>
		/// The Denied drop cursor hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 DeniedDropCursorHotSpot = new Vector2(4, 2);

		/// <summary>
		/// The default cursor texture.
		/// </summary>
		[SerializeField]
		[Obsolete("Replaced with UICursorSettings component.")]
		public Texture2D DefaultCursorTexture;

		/// <summary>
		/// The default cursor hot spot.
		/// </summary>
		[SerializeField]
		[Obsolete("Replaced with UICursorSettings component.")]
		public Vector2 DefaultCursorHotSpot;

		[SerializeField]
		[FormerlySerializedAs("dragHandle")]
		DragSupportHandle handle;

		/// <summary>
		/// Drag handle.
		/// </summary>
		public DragSupportHandle Handle
		{
			get
			{
				return handle;
			}

			set
			{
				if (handle != null)
				{
					handle.OnInitializePotentialDragEvent.RemoveListener(OnInitializePotentialDrag);
					handle.OnBeginDragEvent.RemoveListener(OnBeginDrag);
					handle.OnDragEvent.RemoveListener(OnDrag);
					handle.OnEndDragEvent.RemoveListener(OnEndDrag);
				}

				handle = (value != null)
					? value
					: Utilities.GetOrAddComponent<DragSupportHandle>(this);

				if (handle != null)
				{
					handle.OnInitializePotentialDragEvent.AddListener(OnInitializePotentialDrag);
					handle.OnBeginDragEvent.AddListener(OnBeginDrag);
					handle.OnDragEvent.AddListener(OnDrag);
					handle.OnEndDragEvent.AddListener(OnEndDrag);
				}
			}
		}

		/// <summary>
		/// Drag handle.
		/// </summary>
		[Obsolete("Renamed to Handle.")]
		public DragSupportHandle DragHandle
		{
			get
			{
				return Handle;
			}

			set
			{
				Handle = value;
			}
		}

		/// <summary>
		/// Event on start drag.
		/// </summary>
		[SerializeField]
		public UnityEvent StartDragEvent = new UnityEvent();

		/// <summary>
		/// Event on end drag.
		/// </summary>
		[SerializeField]
		public UnityEvent EndDragEvent = new UnityEvent();

		bool isInited;

		/// <summary>
		/// The current drop target.
		/// </summary>
		protected IDropSupport<T> CurrentTarget;

		/// <summary>
		/// The current drop target.
		/// </summary>
		protected IAutoScroll CurrentAutoScrollTarget;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

#pragma warning disable 0618
			if (DefaultCursorTexture != null)
			{
				UICursor.DefaultCursor = DefaultCursorTexture;
				UICursor.DefaultCursorHotSpot = DefaultCursorHotSpot;
				Debug.LogWarning("DefaultCursorTexture is obsolete and replaced with UICursorSettings component.");
			}
#pragma warning restore 0618

			Handle = handle;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Determines whether this instance can be dragged.
		/// </summary>
		/// <returns><c>true</c> if this instance can be dragged; otherwise, <c>false</c>.</returns>
		/// <param name="eventData">Current event data.</param>
		public virtual bool CanDrag(PointerEventData eventData)
		{
			return AllowDrag;
		}

		/// <summary>
		/// Set Data, which will be passed to Drop component.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		protected abstract void InitDrag(PointerEventData eventData);

		/// <summary>
		/// Called when drop completed.
		/// </summary>
		/// <param name="success"><c>true</c> if Drop component received data; otherwise, <c>false</c>.</param>
		public virtual void Dropped(bool success)
		{
			Data = default(T);
		}

		/// <summary>
		/// If this object is dragged?
		/// </summary>
		protected bool IsDragged;

		/// <summary>
		/// Process OnInitializePotentialDrag event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		protected virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			CurrentTarget = null;
		}

		/// <summary>
		/// Process OnBeginDrag event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		protected virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (CanDrag(eventData))
			{
				StartDragEvent.Invoke();

				EventSystem.current.SetSelectedGameObject(gameObject);
				IsDragged = true;
				InitDrag(eventData);

				FindCurrentTarget(eventData);
			}
		}

		/// <summary>
		/// Find current target.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		protected virtual void FindCurrentTarget(PointerEventData eventData)
		{
			var new_target = FindTarget(eventData);

			if (new_target != CurrentTarget)
			{
				if (CurrentTarget != null)
				{
					CurrentTarget.DropCanceled(Data, eventData);
				}

				OnTargetChanged(CurrentTarget, new_target);
			}

			if (UICursor.CanSet(this))
			{
				if (new_target != null)
				{
					// set cursor can drop
					UICursor.Set(this, AllowDropCursor, AllowDropCursorHotSpot);
				}
				else
				{
					// set cursor fail drop
					UICursor.Set(this, DeniedDropCursor, DeniedDropCursorHotSpot);
				}
			}

			CurrentTarget = new_target;
		}

		/// <summary>
		/// Process OnDrag event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		protected virtual void OnDrag(PointerEventData eventData)
		{
			if (!IsDragged)
			{
				return;
			}

			FindCurrentTarget(eventData);

			Vector2 point;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasTransform as RectTransform, eventData.position, eventData.pressEventCamera, out point))
			{
				return;
			}

			DragPoint.localPosition = point;

			if (CurrentAutoScrollTarget != null)
			{
				CurrentAutoScrollTarget.AutoScrollStop();
			}

			CurrentAutoScrollTarget = FindAutoScrollTarget(eventData);

			if (CurrentAutoScrollTarget != null)
			{
				CurrentAutoScrollTarget.AutoScrollStart(eventData, OnDrag);
			}
		}

		/// <summary>
		/// Process current drop target changed event.
		/// </summary>
		/// <param name="old">Previous drop target.</param>
		/// <param name="current">Current drop target.</param>
		protected virtual void OnTargetChanged(IDropSupport<T> old, IDropSupport<T> current)
		{
		}

		readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

		/// <summary>
		/// Finds the target.
		/// </summary>
		/// <returns>The target.</returns>
		/// <param name="eventData">Event data.</param>
		protected virtual IDropSupport<T> FindTarget(PointerEventData eventData)
		{
			raycastResults.Clear();

			EventSystem.current.RaycastAll(eventData, raycastResults);

			foreach (var raycastResult in raycastResults)
			{
				if (!raycastResult.isValid)
				{
					continue;
				}

				#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
				var target = raycastResult.gameObject.GetComponent<IDropSupport<T>>();
				#else
				var target = raycastResult.gameObject.GetComponent(typeof(IDropSupport<T>)) as IDropSupport<T>;
				#endif
				if (target != null)
				{
					return CheckTarget(target, eventData) ? target : null;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the auto-scroll target.
		/// </summary>
		/// <returns>The auto-scroll  target.</returns>
		/// <param name="eventData">Event data.</param>
		protected virtual IAutoScroll FindAutoScrollTarget(PointerEventData eventData)
		{
			raycastResults.Clear();

			EventSystem.current.RaycastAll(eventData, raycastResults);

			foreach (var raycastResult in raycastResults)
			{
				if (!raycastResult.isValid)
				{
					continue;
				}

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
				var target = raycastResult.gameObject.GetComponent<IAutoScroll>();
#else
				var target = raycastResult.gameObject.GetComponent(typeof(IAutoScroll)) as IAutoScroll;
#endif
				if (target != null)
				{
					return target;
				}
			}

			return null;
		}

		/// <summary>
		/// Check if target can receive drop.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="eventData">Event data.</param>
		/// <returns>true if target can receive drop; otherwise false.</returns>
		protected virtual bool CheckTarget(IDropSupport<T> target, PointerEventData eventData)
		{
			return target.CanReceiveDrop(Data, eventData);
		}

		/// <summary>
		/// Process OnEndDrag event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		protected virtual void OnEndDrag(PointerEventData eventData)
		{
			if (!IsDragged)
			{
				return;
			}

			FindCurrentTarget(eventData);

			if (CurrentTarget != null)
			{
				CurrentTarget.Drop(Data, eventData);

				Dropped(true);
			}
			else
			{
				Dropped(false);
			}

			if (CurrentAutoScrollTarget != null)
			{
				CurrentAutoScrollTarget.AutoScrollStop();
				CurrentAutoScrollTarget = null;
			}

			ResetCursor();

			EndDragEvent.Invoke();
		}

		/// <summary>
		/// Process disable event.
		/// </summary>
		protected virtual void OnDisable()
		{
			if (!IsDragged)
			{
				return;
			}

			if (CurrentTarget != null)
			{
				CurrentTarget.DropCanceled(Data, null);
			}

			Dropped(false);

			if (CurrentAutoScrollTarget != null)
			{
				CurrentAutoScrollTarget.AutoScrollStop();
				CurrentAutoScrollTarget = null;
			}

			ResetCursor();

			EndDragEvent.Invoke();
		}

		/// <summary>
		/// Reset cursor.
		/// </summary>
		protected void ResetCursor()
		{
			IsDragged = false;
			UICursor.Reset(this);
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if ((DragPoints != null) && (CanvasTransform != null) && DragPoints.ContainsKey(CanvasTransform.GetInstanceID()))
			{
				DragPoints.Remove(CanvasTransform.GetInstanceID());
			}
		}

		/// <summary>
		/// Process the cancel event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnCancel(BaseEventData eventData)
		{
			if (!IsDragged)
			{
				return;
			}

			if (CurrentTarget != null)
			{
				CurrentTarget.DropCanceled(Data, null);
			}

			Dropped(false);

			ResetCursor();
		}
	}
}