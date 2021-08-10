namespace UIWidgets
{
	using System;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <content>
	/// Base class for custom ListViews.
	/// </content>
	public partial class ListViewCustom<TComponent, TItem> : ListViewCustomBase, IStylable
		where TComponent : ListViewItem
	{
		/// <summary>
		/// ListView ellipse renderer with items of fixed size.
		/// </summary>
		protected class ListViewTypeEllipse : ListViewTypeBase
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ListViewTypeEllipse"/> class.
			/// </summary>
			/// <param name="owner">Owner.</param>
			public ListViewTypeEllipse(ListViewCustom<TComponent, TItem> owner)
				: base(owner)
			{
				if (Owner.Layout == null)
				{
					Debug.LogWarning("TileViewStaggered requires Container.EasyLayout component.", Owner);
					return;
				}

				if (Owner.Layout.LayoutType != EasyLayoutNS.LayoutTypes.Ellipse)
				{
					// Owner.Layout.LayoutType = EasyLayoutNS.LayoutTypes.Ellipse;
					Debug.LogWarning("EasyLayout type should be set to Ellipse when used ListViewEllipse.", Owner);
				}

				if (Owner.Layout.EllipseSettings.Fill != EasyLayoutNS.EllipseFill.Arc)
				{
					// Owner.Layout.EllipseSettings.Fill = EasyLayoutNS.EllipseFill.Arc;
					Debug.LogWarning("EasyLayout.EllipseSettings.Fill should be set to Arc when used ListViewEllipse.", Owner);
				}

				if (Owner.Layout.EllipseSettings.AngleStepAuto)
				{
					// Owner.Layout.EllipseSettings.AngleStepAuto = false;
					Debug.LogWarning("EasyLayout.EllipseSettings.AngleStepAuto should be disabled when used ListViewEllipse.", Owner);
				}
			}

			/// <summary>
			/// Allow owner to set ContentSizeFitter settings.
			/// </summary>
			public override bool AllowSetContentSizeFitter
			{
				get
				{
					return false;
				}
			}

			/// <summary>
			/// Allow owner to control Container.RectTransform.
			/// </summary>
			public override bool AllowControlRectTransform
			{
				get
				{
					return false;
				}
			}

			/// <summary>
			/// Gets the size of the item.
			/// </summary>
			/// <returns>The item size.</returns>
			protected float GetItemSize()
			{
				return Owner.IsHorizontal()
					? Owner.ItemSize.x + Owner.LayoutBridge.GetSpacing()
					: Owner.ItemSize.y + Owner.LayoutBridge.GetSpacing();
			}

			/// <summary>
			/// Calculates the size of the top filler.
			/// </summary>
			/// <returns>The top filler size.</returns>
			public override float TopFillerSize()
			{
				var settings = Owner.Layout.EllipseSettings;
				var steps = (settings.AngleScroll < 0f)
					? Mathf.CeilToInt(-settings.AngleScroll / settings.AngleStep)
					: -Mathf.FloorToInt(settings.AngleScroll / settings.AngleStep);

				return steps * settings.AngleStep;
			}

			/// <summary>
			/// Calculates the size of the bottom filler.
			/// </summary>
			/// <returns>The bottom filler size.</returns>
			public override float BottomFillerSize()
			{
				return 0f;
			}

			/// <summary>
			/// Gets the first index of the visible.
			/// </summary>
			/// <returns>The first visible index.</returns>
			/// <param name="strict">If set to <c>true</c> strict.</param>
			public override int GetFirstVisibleIndex(bool strict = false)
			{
				var length = Owner.Layout.EllipseSettings.AngleScroll;
				var step = Owner.Layout.EllipseSettings.AngleStep;

				int first_visible_index;
				if (length < 0)
				{
					first_visible_index = strict
						? -Mathf.FloorToInt(-length / step)
						: -Mathf.CeilToInt(-length / step);
				}
				else
				{
					first_visible_index = strict
						? Mathf.CeilToInt(length / step)
						: Mathf.FloorToInt(length / step);
				}

				if (Owner.LoopedListAvailable)
				{
					return first_visible_index;
				}

				first_visible_index = Mathf.Max(0, first_visible_index);
				if (strict)
				{
					return first_visible_index;
				}

				first_visible_index = Mathf.Min(first_visible_index, Mathf.Max(0, Owner.DataSource.Count - MinVisibleItems));
				return first_visible_index;
			}

			/// <summary>
			/// Gets the last visible index.
			/// </summary>
			/// <returns>The last visible index.</returns>
			/// <param name="strict">If set to <c>true</c> strict.</param>
			public override int GetLastVisibleIndex(bool strict = false)
			{
				var length = Owner.Layout.EllipseSettings.AngleScroll + Owner.Layout.EllipseSettings.ArcLength;
				var step = Owner.Layout.EllipseSettings.AngleStep;

				var last_visible_index = strict
					? Mathf.FloorToInt(length / step)
					: Mathf.CeilToInt(length / step);

				return last_visible_index - 1;
			}

			/// <summary>
			/// Gets the position of the start border of the item.
			/// </summary>
			/// <returns>The position.</returns>
			/// <param name="index">Index.</param>
			public override float GetItemPosition(int index)
			{
				var block_index = GetBlockIndex(index);
				return block_index * Owner.Layout.EllipseSettings.AngleStep;
			}

			/// <summary>
			/// Gets the position of the end border of the item.
			/// </summary>
			/// <returns>The position.</returns>
			/// <param name="index">Index.</param>
			public override float GetItemPositionBorderEnd(int index)
			{
				return GetItemPosition(index + 1);
			}

			/// <summary>
			/// Gets the position to display item at the center of the ScrollRect viewport.
			/// </summary>
			/// <returns>The position.</returns>
			/// <param name="index">Index.</param>
			public override float GetItemPositionMiddle(int index)
			{
				return GetItemPosition(index) - (Owner.Layout.EllipseSettings.ArcLength / 2f);
			}

			/// <summary>
			/// Gets the position to display item at the bottom of the ScrollRect viewport.
			/// </summary>
			/// <returns>The position.</returns>
			/// <param name="index">Index.</param>
			public override float GetItemPositionBottom(int index)
			{
				return GetItemPosition(index) - Owner.Layout.EllipseSettings.ArcLength;
			}

			/// <summary>
			/// Calculates the maximum count of the visible items.
			/// </summary>
			public override void CalculateMaxVisibleItems()
			{
				if (!Owner.Virtualization)
				{
					MaxVisibleItems = Owner.DataSource.Count;
					return;
				}

				var length = Owner.Layout.EllipseSettings.ArcLength;
				var step = Owner.Layout.EllipseSettings.AngleStep;

				var max = Mathf.CeilToInt(length / step);

				MaxVisibleItems = Mathf.Max(max + 2, MinVisibleItems);
			}

			/// <summary>
			/// Gets the index of the nearest item.
			/// </summary>
			/// <returns>The nearest item index.</returns>
			/// <param name="point">Point.</param>
			/// <param name="type">Preferable nearest index.</param>
			public override int GetNearestIndex(Vector2 point, NearestType type)
			{
				if (Owner.IsSortEnabled())
				{
					return 0;
				}

				if (Owner.Components.Count == 0)
				{
					return 0;
				}

				if (Owner.Components.Count == 1)
				{
					switch (type)
					{
						case NearestType.Auto:
							return 0;
						case NearestType.Before:
							return 0;
						case NearestType.After:
							return 1;
						default:
							throw new NotSupportedException("Unknown position: " + type);
					}
				}

				int index;

				var nearest = 0;
				var minimal_distance = GetDistance(nearest, point);
				var base_distance = BaseDistance();
				switch (type)
				{
					case NearestType.Auto:
						for (int i = 1; i < Owner.Components.Count; i++)
						{
							var new_distance = GetDistance(i, point);
							if (new_distance < minimal_distance)
							{
								minimal_distance = new_distance;
								nearest = i;
							}
						}

						index = Owner.Components[nearest].Index;
						break;
					case NearestType.Before:
						for (int i = 1; i < Owner.Components.Count; i++)
						{
							var new_distance = GetDistance(i, point);
							if ((new_distance < minimal_distance) && (new_distance < (base_distance / 2f)))
							{
								minimal_distance = new_distance;
								nearest = i;
							}
						}

						index = Owner.Components[nearest].Index;
						break;
					case NearestType.After:
						for (int i = 1; i < Owner.Components.Count; i++)
						{
							var new_distance = GetDistance(i, point);
							if ((new_distance < minimal_distance) && (new_distance < (base_distance / 2f)))
							{
								minimal_distance = new_distance;
								nearest = i;
							}
						}

						index = Owner.Components[nearest].Index + 1;
						break;
					default:
						throw new NotSupportedException("Unsupported NearestType: " + type);
				}

				return index;
			}

			float BaseDistance()
			{
				return Vector2.Distance(
					(Owner.Components[0].transform as RectTransform).localPosition,
					(Owner.Components[1].transform as RectTransform).localPosition);
			}

			float GetDistance(int index, Vector2 point)
			{
				var pos = (Owner.Components[index].transform as RectTransform).localPosition;
				return Vector2.Distance(pos, point);
			}

			/// <summary>
			/// Gets the index of the nearest item.
			/// </summary>
			/// <returns>The nearest item index.</returns>
			public override int GetNearestItemIndex()
			{
				if (Owner.DataSource.Count == 0)
				{
					return -1;
				}

				return VisibleIndex2ItemIndex(Visible.FirstVisible);
			}

			/// <summary>
			/// Get the size of the ListView.
			/// </summary>
			/// <returns>The size.</returns>
			public override float ListSize()
			{
				if (Owner.DataSource.Count == 0)
				{
					return 0;
				}

				return (Owner.DataSource.Count - 1) * Owner.Layout.EllipseSettings.AngleStep;
			}

			/// <summary>
			/// Validates the content size and item size.
			/// </summary>
			public override void ValidateContentSize()
			{
			}

			bool isEnabled;

			EasyLayoutNS.EasyLayoutEllipseScroll ellipseScroll;

			/// <summary>
			/// Ellipse scroll listener.
			/// </summary>
			protected EasyLayoutNS.EasyLayoutEllipseScroll EllipseScroll
			{
				get
				{
					if (ellipseScroll == null)
					{
						ellipseScroll = Utilities.GetOrAddComponent<EasyLayoutNS.EasyLayoutEllipseScroll>(Owner.Container);
					}

					return ellipseScroll;
				}
			}

			/// <summary>
			/// Allow looped ListView.
			/// </summary>
			public override bool AllowLoopedList
			{
				get
				{
					return !IsTileView && (Owner.Layout.EllipseSettings.ArcLength < ListSize());
				}
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

				EllipseScroll.OnScrollEvent.AddListener(UpdateView);
				isEnabled = true;
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

				EllipseScroll.OnScrollEvent.RemoveListener(UpdateView);
				isEnabled = false;
			}

			ScrollListener scrollListener;

			/// <summary>
			/// Update view.
			/// </summary>
			public override void UpdateView()
			{
				base.UpdateView();

				scrollListener.ScrollEvent.Invoke(new PointerEventData(EventSystem.current));
			}

			/// <summary>
			/// Reset position.
			/// </summary>
			public override void ResetPosition()
			{
				if (Owner.scrollRect != null)
				{
					Owner.scrollRect.horizontal = false;
					Owner.scrollRect.vertical = false;
					Owner.scrollRect.StopMovement();
					Owner.scrollRect.content.anchoredPosition = Vector2.zero;
					scrollListener = Utilities.GetOrAddComponent<ScrollListener>(Owner.ScrollRect);
				}

				if (Owner.Layout != null)
				{
					Owner.Layout.EllipseSettings.AngleScroll = -Owner.Layout.EllipseSettings.ArcLength / 2f;
				}
			}

			/// <summary>
			/// Validate position.
			/// </summary>
			protected override void ValidatePosition()
			{
				var base_position = GetPosition();
				var position = ValidatePosition(base_position);
				if (base_position != position)
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
				var list_size = ListSize();
				if (IsRequiredCenterTheItems())
				{
					position = 0f;
				}
				else if (Owner.LoopedListAvailable)
				{
				}
				else
				{
					var half_arc = Owner.Layout.EllipseSettings.ArcLength / 2f;
					list_size -= half_arc;
					if (position > list_size)
					{
						position = list_size;
					}
					else if (position < -half_arc)
					{
						position = -half_arc;
					}
				}

				return position;
			}

			/// <summary>
			/// Set the position.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <param name="updateView">Update view if position changed.</param>
			public override void SetPosition(float value, bool updateView = true)
			{
				value = ValidatePosition(value);
				if (Owner.Layout.EllipseSettings.AngleScroll != value)
				{
					Owner.Layout.EllipseSettings.AngleScroll = value;

					if (updateView)
					{
						UpdateView();
					}
				}
			}

			/// <summary>
			/// Set the position.
			/// </summary>
			/// <param name="newPosition">Value.</param>
			/// <param name="updateView">Update view if position changed.</param>
			public override void SetPosition(Vector2 newPosition, bool updateView = true)
			{
				var value = Owner.IsHorizontal() ? newPosition.x : newPosition.y;
				SetPosition(value, updateView);
			}

			/// <summary>
			/// Get the position.
			/// </summary>
			/// <returns>Position.</returns>
			public override Vector2 GetPositionVector()
			{
				return Owner.IsHorizontal()
					? new Vector2(Owner.Layout.EllipseSettings.AngleScroll, 0f)
					: new Vector2(0f, Owner.Layout.EllipseSettings.AngleScroll);
			}

			/// <summary>
			/// Get the position.
			/// </summary>
			/// <returns>Position.</returns>
			public override float GetPosition()
			{
				return Owner.Layout.EllipseSettings.AngleScroll;
			}

			/// <summary>
			/// Get the position for the specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <returns>Position.</returns>
			public override Vector2 GetPosition(int index)
			{
				var pos = ValidatePosition(GetItemPosition(index));

				return Owner.IsHorizontal()
					? new Vector2(pos, 0f)
					: new Vector2(0f, pos);
			}

			/// <summary>
			/// Is visible item with specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <param name="minVisiblePart">The minimal visible part of the item to consider item visible.</param>
			/// <returns>true if item visible; false otherwise.</returns>
			public override bool IsVisible(int index, float minVisiblePart)
			{
				return Owner.DisplayedIndices.Contains(index);
			}

			/// <summary>
			/// Get the top filler size to center the items.
			/// </summary>
			/// <returns>Size.</returns>
			public override float CenteredFillerSize()
			{
				return -(Owner.Layout.EllipseSettings.ArcLength - ListSize()) / 2f;
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

				return Owner.Layout.EllipseSettings.ArcLength > ListSize();
			}

			/// <summary>
			/// Updates the layout bridge.
			/// </summary>
			public override void UpdateLayout()
			{
				if (Owner.LayoutBridge == null)
				{
					return;
				}

				var settings = Owner.Layout.EllipseSettings;
				if (IsRequiredCenterTheItems())
				{
					settings.AngleFiller = CenteredFillerSize();
				}
				else if (Owner.LoopedListAvailable)
				{
					settings.AngleFiller = TopFillerSize();
				}
				else
				{
					settings.AngleFiller = settings.AngleScroll > 0f ? TopFillerSize() : 0f;
				}
			}
		}
	}
}