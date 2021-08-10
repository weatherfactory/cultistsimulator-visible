namespace UIWidgets
{
	using System;
	using UIWidgets.Styles;
	using UnityEngine;

	/// <content>
	/// Base class for custom ListViews.
	/// </content>
	public partial class ListViewCustom<TComponent, TItem> : ListViewCustomBase, IStylable
		where TComponent : ListViewItem
	{
		/// <summary>
		/// ListView renderer with items of fixed size.
		/// </summary>
		protected abstract class ListViewTypeRectangle : ListViewTypeBase
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ListViewTypeRectangle"/> class.
			/// </summary>
			/// <param name="owner">Owner.</param>
			protected ListViewTypeRectangle(ListViewCustom<TComponent, TItem> owner)
				: base(owner)
			{
			}

			bool isEnabled;

			/// <summary>
			/// Allow looped ListView.
			/// </summary>
			public override bool AllowLoopedList
			{
				get
				{
					return !IsTileView && (GetScrollRectSize() < ListSize());
				}
			}

			/// <summary>
			/// Determines whether is required center the list items.
			/// </summary>
			/// <returns><c>true</c> if required center the list items; otherwise, <c>false</c>.</returns>
			public override bool IsRequiredCenterTheItems()
			{
				if (!Owner.CenterTheItems)
				{
					return false;
				}

				return GetScrollRectSize() > ListSize();
			}

			/// <summary>
			/// Gets the size of the scroll.
			/// </summary>
			/// <returns>The scroll size.</returns>
			protected float GetScrollRectSize()
			{
				return Owner.IsHorizontal() ? Owner.ScrollRectSize.x : Owner.ScrollRectSize.y;
			}

			/// <summary>
			/// Get the top filler size to center the items.
			/// </summary>
			/// <returns>Size.</returns>
			public override float CenteredFillerSize()
			{
				return (GetScrollRectSize() - ListSize() - Owner.LayoutBridge.GetFullMargin()) / 2f;
			}

			/// <summary>
			/// Enable this instance.
			/// </summary>
			public override void Enable()
			{
				if (isEnabled)
				{
					return;
				}

				if (Owner.ScrollRect != null)
				{
					Owner.ScrollRect.onValueChanged.AddListener(OnScroll);
					isEnabled = true;
				}
			}

			/// <summary>
			/// Disable this instance.
			/// </summary>
			public override void Disable()
			{
				if (!isEnabled)
				{
					return;
				}

				if (Owner.ScrollRect != null)
				{
					Owner.ScrollRect.onValueChanged.RemoveListener(OnScroll);
					isEnabled = false;
				}
			}

			/// <summary>
			/// Reset position.
			/// </summary>
			public override void ResetPosition()
			{
				if (Owner.scrollRect != null)
				{
					Owner.scrollRect.horizontal = Owner.IsHorizontal();
					Owner.scrollRect.vertical = !Owner.IsHorizontal();
					Owner.scrollRect.StopMovement();
					Owner.scrollRect.content.anchoredPosition = Vector2.zero;
				}
			}

			/// <summary>
			/// Process scroll event.
			/// </summary>
			/// <param name="unused">Scroll value.</param>
			protected void OnScroll(Vector2 unused)
			{
				UpdateView();
			}

			/// <summary>
			/// Validate position.
			/// </summary>
			protected override void ValidatePosition()
			{
				var base_position = GetPosition();
				var position = ValidatePosition(base_position);
				if (!Mathf.Approximately(base_position, position))
				{
					SetPosition(position, false);
				}
			}

			/// <summary>
			/// Validate position.
			/// </summary>
			/// <param name="position">Position.</param>
			/// <returns>Validated position.</returns>
			public override float ValidatePosition(float position)
			{
				if (!Owner.LoopedListAvailable)
				{
					return position;
				}

				var list_size = ListSize() + Owner.LayoutBridge.GetFullMargin();
				if (Owner.IsHorizontal())
				{
					if (position < -list_size)
					{
						position += list_size;
					}
					else if (position > 0f)
					{
						position -= list_size;
					}
				}
				else
				{
					if (position > list_size)
					{
						position -= list_size;
					}
					else if (position < 0f)
					{
						position += list_size;
					}
				}

				return position;
			}

			/// <summary>
			/// Sets the scroll value.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <param name="updateView">Update view if position changed.</param>
			public override void SetPosition(float value, bool updateView = true)
			{
				if ((Owner.ScrollRect == null) || Owner.ScrollRect.content == null)
				{
					return;
				}

				var current_position = Owner.ScrollRect.content.anchoredPosition;
				var new_position = Owner.IsHorizontal()
					? new Vector2(-value, current_position.y)
					: new Vector2(current_position.x, value);

				SetPosition(new_position, updateView);
			}

			/// <summary>
			/// Sets the scroll value.
			/// </summary>
			/// <param name="newPosition">Value.</param>
			/// <param name="updateView">Update view if position changed.</param>
			public override void SetPosition(Vector2 newPosition, bool updateView = true)
			{
				if ((Owner.ScrollRect == null) || Owner.ScrollRect.content == null)
				{
					return;
				}

				newPosition = ValidatePosition(newPosition);

				var current_position = Owner.ScrollRect.content.anchoredPosition;
				var diff = (Owner.IsHorizontal() && !Mathf.Approximately(current_position.x, newPosition.x))
						|| (!Owner.IsHorizontal() && !Mathf.Approximately(current_position.y, newPosition.y));

				Owner.ScrollRect.StopMovement();

				if (diff)
				{
					Owner.ScrollRect.content.anchoredPosition = newPosition;

					if (updateView)
					{
						UpdateView();
					}
				}

				Owner.ScrollRect.StopMovement();
			}

			/// <summary>
			/// Gets the scroll value in ListView direction.
			/// </summary>
			/// <returns>The scroll value.</returns>
			public override Vector2 GetPositionVector()
			{
				var result = Owner.ScrollRect.content.anchoredPosition;
				if (Owner.IsHorizontal())
				{
					result.x = -result.x;
				}

				if (Owner.LoopedListAvailable)
				{
					return result;
				}

				if (float.IsNaN(result.x))
				{
					result.x = 0f;
				}

				if (float.IsNaN(result.y))
				{
					result.y = 0f;
				}

				return result;
			}

			/// <summary>
			/// Gets the scroll value in ListView direction.
			/// </summary>
			/// <returns>The scroll value.</returns>
			public override float GetPosition()
			{
				var pos = GetPositionVector();
				return Owner.IsHorizontal() ? pos.x : pos.y;
			}

			/// <summary>
			/// Get scroll position for the specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <returns>Scroll position</returns>
			public override Vector2 GetPosition(int index)
			{
				var scroll_main = GetPosition();

				var item_starts = GetItemPosition(index);
				var item_ends = GetItemPositionBottom(index);

				if (item_starts < scroll_main)
				{
					scroll_main = item_starts;
				}
				else if (item_ends > scroll_main)
				{
					scroll_main = item_ends;
				}

				var scroll_secondary = Owner.GetScrollPositionSecondary(index);

				var position = Owner.IsHorizontal()
					? new Vector2(ValidatePosition(-scroll_main), scroll_secondary)
					: new Vector2(scroll_secondary, ValidatePosition(scroll_main));

				return position;
			}

			/// <summary>
			/// Is visible item with specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <param name="minVisiblePart">The minimal visible part of the item to consider item visible.</param>
			/// <returns>true if item visible; false otherwise.</returns>
			public override bool IsVisible(int index, float minVisiblePart)
			{
				if (!Owner.IsValid(index))
				{
					return false;
				}

				var viewport_top = GetPosition() + Owner.LayoutBridge.GetMargin();
				var viewport_bottom = viewport_top + GetScrollRectSize() - Owner.LayoutBridge.GetMargin();

				var border_top = GetItemPosition(index);
				var border_bottom = GetItemPositionBorderEnd(index);
				var size = border_bottom - border_top;

				var border_top_visible = (viewport_top <= border_top) && (border_top < viewport_bottom);
				if (border_top_visible)
				{
					var visible_top = Mathf.Max(viewport_top, border_top);
					var visible_bottom = Mathf.Min(viewport_bottom, border_top + size);
					var visible_part = (visible_bottom - visible_top) / size;

					return visible_part >= minVisiblePart;
				}

				var border_bottom_visible = (viewport_top <= border_bottom) && (border_bottom < viewport_bottom);
				if (border_bottom_visible)
				{
					var visible_top = Mathf.Max(viewport_top, border_bottom - size);
					var visible_bottom = Mathf.Min(viewport_bottom, viewport_bottom);
					var visible_part = (visible_bottom - visible_top) / size;

					return visible_part >= minVisiblePart;
				}

				return false;
			}

			/// <summary>
			/// Updates the layout bridge.
			/// </summary>
			public override void UpdateLayout()
			{
				if (!Owner.Virtualization)
				{
					Owner.LayoutBridge.SetFiller(0f, 0f);
					Owner.LayoutBridge.UpdateLayout();
					return;
				}

				if (IsRequiredCenterTheItems())
				{
					var filler = CenteredFillerSize();
					Owner.LayoutBridge.SetFiller(filler, 0f);
				}
				else
				{
					var top = TopFillerSize();
					var bottom = BottomFillerSize();
					Owner.LayoutBridge.SetFiller(top, bottom);
				}

				Owner.LayoutBridge.UpdateLayout();
			}
		}
	}
}