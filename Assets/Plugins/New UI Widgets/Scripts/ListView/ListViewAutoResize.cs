namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Auto-resize ListView or TileView according to items counts until specified maximum size reached.
	/// </summary>
	[RequireComponent(typeof(ListViewBase))]
	public class ListViewAutoResize : MonoBehaviour
	{
		/// <summary>
		/// Maximum size.
		/// </summary>
		[SerializeField]
		public float MaxSize = 250f;

		/// <summary>
		/// Update RectTransform.
		/// </summary>
		[SerializeField]
		public bool UpdateRectTransform = true;

		/// <summary>
		/// Update LayoutElement.
		/// </summary>
		[SerializeField]
		public bool UpdateLayoutElement = true;

		ListViewBase listView;

		/// <summary>
		/// ListView.
		/// </summary>
		protected ListViewBase ListView
		{
			get
			{
				if (listView == null)
				{
					listView = GetComponent<ListViewBase>();
				}

				return listView;
			}
		}

		LayoutElement layoutElement;

		/// <summary>
		/// LayoutElement.
		/// </summary>
		protected LayoutElement LayoutElement
		{
			get
			{
				if (layoutElement == null)
				{
					layoutElement = Utilities.GetOrAddComponent<LayoutElement>(this);
				}

				return layoutElement;
			}
		}

		/// <summary>
		/// RectTransform.
		/// </summary>
		protected RectTransform RectTransform;

		/// <summary>
		/// Difference in size between ListView and ListView.Container parent.
		/// </summary>
		[NonSerialized]
		protected float BaseSizeDelta = 2f;

		/// <summary>
		/// EasyLayout start margin.
		/// </summary>
		[NonSerialized]
		protected float LayoutMarginStart = 0f;

		/// <summary>
		/// EasyLayout end margin.
		/// </summary>
		[NonSerialized]
		protected float LayoutMarginEnd = 0f;

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

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

			var scroll_rect = ListView.GetScrollRect();
			var layout = scroll_rect.content.GetComponent<EasyLayoutNS.EasyLayout>();
			LayoutMarginStart = ListView.IsHorizontal() ? layout.MarginLeft : layout.MarginTop;
			LayoutMarginEnd = ListView.IsHorizontal() ? layout.MarginRight : layout.MarginBottom;
			RectTransform = transform as RectTransform;

			var content_parent = scroll_rect.content.parent as RectTransform;
			if (ListView.IsHorizontal())
			{
				BaseSizeDelta = RectTransform.rect.width - content_parent.rect.width;
			}
			else
			{
				BaseSizeDelta = RectTransform.rect.height - content_parent.rect.height;
			}

			ListView.OnUpdateView.AddListener(Resize);
			Resize();
		}

		/// <summary>
		/// Resize.
		/// </summary>
		protected virtual void Resize()
		{
			var count = ListView.GetItemsCount();
			var size = count == 0
				? LayoutMarginStart
				: ListView.GetItemPositionBorderEnd(count - 1);
			size += LayoutMarginEnd + BaseSizeDelta;
			size = Mathf.Min(size, MaxSize);

			if (UpdateRectTransform)
			{
				var axis = ListView.IsHorizontal() ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
				RectTransform.SetSizeWithCurrentAnchors(axis, size);
			}

			if (UpdateRectTransform)
			{
				if (ListView.IsHorizontal())
				{
					LayoutElement.minWidth = size;
				}
				else
				{
					LayoutElement.minHeight = size;
				}
			}
		}
	}
}