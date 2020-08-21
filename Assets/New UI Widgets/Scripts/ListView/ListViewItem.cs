﻿// <auto-generated/>
// Auto-generated added to suppress names errors.

namespace UIWidgets
{
	using System.Collections.Generic;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewItem.
	/// Item for ListViewBase.
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class ListViewItem : UIBehaviour,
		IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
		IPointerEnterHandler, IPointerExitHandler,
		ISubmitHandler, ICancelHandler,
		ISelectHandler, IDeselectHandler,
		IMoveHandler, IStylable, IMovableToCache, IUpgradeable
	{
		/// <summary>
		/// The index of item in ListView.
		/// </summary>
		[HideInInspector]
		public int Index = -1;

		/// <summary>
		/// What to do when the event system send a pointer click event.
		/// </summary>
		public UnityEvent onClick = new UnityEvent();

		/// <summary>
		/// What to do when the event system send a pointer click event.
		/// </summary>
		public ListViewItemSelect onClickItem = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a pointer down event.
		/// </summary>
		public PointerUnityEvent onPointerDown = new PointerUnityEvent();

		/// <summary>
		/// What to do when the event system send a pointer up event.
		/// </summary>
		public PointerUnityEvent onPointerUp = new PointerUnityEvent();

		/// <summary>
		/// What to do when the event system send a submit event.
		/// </summary>
		public ListViewItemSelect onSubmit = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a cancel event.
		/// </summary>
		public ListViewItemSelect onCancel = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a select event.
		/// </summary>
		public ListViewItemSelect onSelect = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a deselect event.
		/// </summary>
		public ListViewItemSelect onDeselect = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a move event.
		/// </summary>
		public ListViewItemMove onMove = new ListViewItemMove();

		/// <summary>
		/// What to do when the event system send a pointer click event.
		/// </summary>
		public PointerUnityEvent onPointerClick = new PointerUnityEvent();

		/// <summary>
		/// What to do when the event system send a pointer enter event.
		/// </summary>
		public PointerUnityEvent onPointerEnter = new PointerUnityEvent();

		/// <summary>
		/// What to do when the event system send a pointer enter event.
		/// </summary>
		public ListViewItemSelect onPointerEnterItem = new ListViewItemSelect();

		/// <summary>
		/// What to do when the event system send a pointer exit event.
		/// </summary>
		public PointerUnityEvent onPointerExit = new PointerUnityEvent();

		/// <summary>
		/// What to do when the event system send a pointer exit event.
		/// </summary>
		public ListViewItemSelect onPointerExitItem = new ListViewItemSelect();

		/// <summary>
		/// OnResize event.
		/// </summary>
		public ListViewItemResize onResize = new ListViewItemResize();

		/// <summary>
		/// OnDoubleClick event.
		/// </summary>
		public ListViewItemClick onDoubleClick = new ListViewItemClick();

		/// <summary>
		/// Parent ListView.
		/// </summary>
		[HideInInspector]
		public ListViewBase Owner;

		Image background;

		/// <summary>
		/// The background.
		/// </summary>
		public Image Background
		{
			get
			{
				if (background == null)
				{
					background = GetComponent<Image>();
				}

				return background;
			}
		}

		RectTransform rectTransform;

		/// <summary>
		/// Gets the RectTransform.
		/// </summary>
		/// <value>The RectTransform.</value>
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
		/// Selectable objects.
		/// </summary>
		[HideInInspector]
		[SerializeField]
		protected List<GameObject> SelectableObjects = new List<GameObject>();

		/// <summary>
		/// Selectable targets.
		/// </summary>
		protected List<ISelectHandler> SelectableTargets = new List<ISelectHandler>();

		/// <summary>
		/// Find selectable GameObjects.
		/// </summary>
		public virtual void FindSelectableObjects()
		{
			SelectableObjects.Clear();
			SelectableTargets.Clear();
			Compatibility.GetComponentsInChildren(this, true, SelectableTargets);

			for (int i = 0; i < SelectableTargets.Count; i++)
			{
				SelectableObjects.Add((SelectableTargets[i] as Component).gameObject);
			}
		}

		/// <summary>
		/// Get index of the specified selectable GameObject.
		/// </summary>
		/// <param name="go">Selectable GameObject</param>
		/// <returns>Index of the specified selectable GameObject.</returns>
		public virtual int GetSelectableIndex(GameObject go)
		{
			return SelectableObjects.IndexOf(go);
		}

		/// <summary>
		/// Get selectable GameObject by specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <returns>Selectable GameObject.</returns>
		public virtual GameObject GetSelectableObject(int index)
		{
			if ((index < 0) || (index >= SelectableObjects.Count))
			{
				return null;
			}

			return SelectableObjects[index];
		}

		#region GraphicsColoring

		/// <summary>
		/// Is colors setted at least once?
		/// </summary>
		protected bool GraphicsColorSetted = false;

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public virtual Graphic[] GraphicsForeground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Background graphics for coloring.
		/// </summary>
		public virtual Graphic[] GraphicsBackground
		{
			get
			{
				return new Graphic[] { Background, };
			}
		}

		/// <summary>
		/// Reset graphics colors.
		/// </summary>
		/// <param name="graphic">Graphic.</param>
		protected virtual void GraphicsReset(Graphic graphic)
		{
			if (graphic != null)
			{
				graphic.color = Color.white;
			}
		}

		/// <summary>
		/// Set the specified color for the graphics.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="color">New color.</param>
		/// <param name="fadeDuration">Time for fade the original color to the new one.</param>
		protected void GraphicsSet(Graphic[] graphics, Color color, float fadeDuration)
		{
			if (graphics == null)
			{
				return;
			}

			// reset default color to white, otherwise it will look darker than specified color,
			// because actual color = graphic.color * graphic.CrossFadeColor
			if (!GraphicsColorSetted)
			{
				GraphicsReset(graphics);
			}

			var duration = GraphicsColorSetted ? fadeDuration : 0f;
			foreach (var g in graphics)
			{
				if (g != null)
				{
					g.CrossFadeColor(color, duration, true, true);
				}
			}
		}

		/// <summary>
		/// Reset graphics colors.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		protected void GraphicsReset(Graphic[] graphics)
		{
			if (graphics == null)
			{
				return;
			}

			graphics.ForEach(GraphicsReset);
		}

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public virtual void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration = 0.0f)
		{
			GraphicsSet(GraphicsForeground, foregroundColor, GraphicsColorSetted ? fadeDuration : 0f);
			GraphicsSet(GraphicsBackground, backgroundColor, GraphicsColorSetted ? fadeDuration : 0f);

			GraphicsColorSetted = true;
		}
		#endregion

		/// <summary>
		/// Is need to set localPosition.z to 0?
		/// </summary>
		[SerializeField]
		protected bool LocalPositionZReset;

		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected override void Awake()
		{
			if (LocalPositionZReset && (transform.localPosition.z != 0f))
			{
				var pos = transform.localPosition;
				pos.z = 0f;
				transform.localPosition = pos;
			}
		}

		/// <summary>
		/// Determines whether owner widget is interactable.
		/// </summary>
		/// <returns><c>true</c> if owner widget is interactable; otherwise, <c>false</c>.</returns>
		public virtual bool IsInteractable()
		{
			if (Owner == null)
			{
				return true;
			}

			return Owner.IsInteractable();
		}

		/// <summary>
		/// Process the pointer down event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public virtual void OnPointerDown(PointerEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onPointerDown.Invoke(eventData);
		}

		/// <summary>
		/// Process the pointer up event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public virtual void OnPointerUp(PointerEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onPointerUp.Invoke(eventData);
		}

		/// <summary>
		/// Process the move event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnMove(AxisEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onMove.Invoke(eventData, this);
		}

		/// <summary>
		/// Process the submit event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onSubmit.Invoke(this);
		}

		/// <summary>
		/// Process the cancel event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnCancel(BaseEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onCancel.Invoke(this);
		}

		/// <summary>
		/// Process the select event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnSelect(BaseEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			Select();
			onSelect.Invoke(this);
		}

		/// <summary>
		/// Process the deselect event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnDeselect(BaseEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onDeselect.Invoke(this);
		}

		/// <summary>
		/// Process the pointer click event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onPointerClick.Invoke(eventData);

			if ((eventData.button == PointerEventData.InputButton.Left) && (eventData.clickCount == 1))
			{
				onClick.Invoke();
				onClickItem.Invoke(this);
				Select();
			}

			if ((eventData.button == PointerEventData.InputButton.Left) && (eventData.clickCount == 2))
			{
				onDoubleClick.Invoke(Index);
			}
		}

		/// <summary>
		/// Process the pointer enter event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onPointerEnter.Invoke(eventData);
			onPointerEnterItem.Invoke(this);
		}

		/// <summary>
		/// Process the pointer exit event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnPointerExit(PointerEventData eventData)
		{
			if (!IsInteractable())
			{
				return;
			}

			onPointerExit.Invoke(eventData);
			onPointerExitItem.Invoke(this);
		}

		/// <summary>
		/// Select this instance.
		/// </summary>
		public virtual void Select()
		{
			if (!IsInteractable())
			{
				return;
			}

			if (EventSystem.current.alreadySelecting)
			{
				return;
			}

			var ev = new ListViewItemEventData(EventSystem.current)
			{
				NewSelectedObject = gameObject
			};
			EventSystem.current.SetSelectedGameObject(ev.NewSelectedObject, ev);
		}

		Rect oldRect;

		/// <summary>
		/// Implementation of a callback that is sent if an associated RectTransform has it's dimensions changed..
		/// </summary>
		protected override void OnRectTransformDimensionsChange()
		{
			if (oldRect.Equals(RectTransform.rect))
			{
				return;
			}

			oldRect = RectTransform.rect;
			onResize.Invoke(Index, oldRect.size);
		}

		/// <summary>
		/// Called when item moved to cache, you can use it free used resources.
		/// </summary>
		public virtual void MovedToCache()
		{
		}

		/// <summary>
		/// Called when item in default state.
		/// </summary>
		public virtual void StateDefault()
		{
		}

		/// <summary>
		/// Called when item selected.
		/// </summary>
		public virtual void StateSelected()
		{
		}

		/// <summary>
		/// Called when item highlighted.
		/// </summary>
		public virtual void StateHighlighted()
		{
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();

			if (!Compatibility.IsPrefab(this))
			{
				Upgrade();
			}
		}
#endif

		#region IStylable implementation

		/// <summary>
		/// Reset graphics.
		/// </summary>
		protected virtual void ResetGraphics()
		{
#if UNITY_EDITOR
			var is_playmode = UnityEditor.EditorApplication.isPlaying;
#else
			var is_playmode = true;
#endif

			if (is_playmode)
			{
				GraphicsReset(GraphicsForeground);
				GraphicsReset(GraphicsBackground);
			}
		}

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="styleBackground">Style for the background.</param>
		/// <param name="styleText">Style for the text.</param>
		/// <param name="style">Full style data.</param>
		public virtual void SetStyle(StyleImage styleBackground, StyleText styleText, Style style)
		{
			styleBackground.ApplyTo(Background);

			if ((Owner!=null) && (Owner.IsTable))
			{
				Background.sprite = null;

				if (GraphicsBackground != null)
				{
					GraphicsBackground.ForEach(style.Table.Background.ApplyTo);
				}
			}

			if (GraphicsForeground != null)
			{
				foreach (var gf in GraphicsForeground)
				{
					if (gf != null)
					{
						styleText.ApplyTo(gf.gameObject);
					}
				}
			}

			ResetGraphics();
		}

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);

			ResetGraphics();

			return true;
		}
		#endregion
	}
}