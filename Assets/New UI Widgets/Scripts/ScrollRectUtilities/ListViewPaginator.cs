namespace UIWidgets
{
	using System;
	using UnityEngine;

	/// <summary>
	/// ListView's paginator. Also works with TileView's.
	/// </summary>
	public class ListViewPaginator : ScrollRectPaginator
	{
		/// <summary>
		/// ListView.
		/// </summary>
		[SerializeField]
		protected ListViewBase ListView;

		/// <summary>
		/// Count of items on one page.
		/// </summary>
		[SerializeField]
		protected int perPage = 1;

		/// <summary>
		/// Gets or sets the count of items on one page.
		/// </summary>
		/// <value>The per page.</value>
		public int PerPage
		{
			get
			{
				return Mathf.Max(1, perPage);
			}

			set
			{
				perPage = Mathf.Max(1, value);
				RecalculatePages();
			}
		}

		bool isListViewPaginatorInited;

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected override void Init()
		{
			if (isListViewPaginatorInited)
			{
				return;
			}

			isListViewPaginatorInited = true;

			ListView.Init();
			ScrollRect = ListView.GetScrollRect();

			base.Init();
		}

		/// <summary>
		/// Get margin to change the last page size to full-page size.
		/// </summary>
		/// <returns>Margin.</returns>
		protected override float GetLastPageMargin()
		{
			var items_per_block = ListView.GetItemsPerBlock();
			var items_per_page = items_per_block * PerPage;
			var items_at_last_page = ListView.GetItemsCount() % items_per_page;
			var unexisted_items = items_per_page - items_at_last_page;
			var unexisted_blocks = Mathf.FloorToInt(unexisted_items / items_per_block);

			var size = ListView.GetDefaultItemSize();

			var margin = 0f;
			if (IsHorizontal())
			{
				margin = unexisted_blocks * size.x;
				if (unexisted_blocks > 0)
				{
					margin += (unexisted_blocks - 1) * Layout.Spacing.x;
				}
			}
			else
			{
				margin = unexisted_blocks * size.y;
				if (unexisted_blocks > 0)
				{
					margin += (unexisted_blocks - 1) * Layout.Spacing.y;
				}
			}

			return margin;
		}

		/// <summary>
		/// Get current page.
		/// </summary>
		/// <returns>Page.</returns>
		protected override int GetPage()
		{
			var position = GetPosition();
			var delta = ScrollRectSize() - GetPageSize();
			switch (ForcedPosition)
			{
				case PaginatorPagePosition.None:
				case PaginatorPagePosition.OnStart:
					break;
				case PaginatorPagePosition.OnCenter:
					position += (IsHorizontal() ? -delta : delta) / 2f;
					break;
				case PaginatorPagePosition.OnEnd:
					position += IsHorizontal() ? -delta : delta;
					break;
				default:
					throw new NotSupportedException("Unknown forced position: " + ForcedPosition);
			}

			var v_pos = IsHorizontal()
				? new Vector2(position, 0f)
				: new Vector2(0f, position);

			var index = ListView.GetNearestIndex(v_pos);

			return Index2Page(index);
		}

		/// <summary>
		/// Convert index to page.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <returns>Page.</returns>
		protected int Index2Page(int index)
		{
			var page = Mathf.RoundToInt(((float)index) / (ListView.GetItemsPerBlock() * PerPage));
			return Mathf.Min(page, Pages - 1);
		}

		/// <summary>
		/// Determines whether direction is horizontal.
		/// </summary>
		/// <returns><c>true</c> if this instance is horizontal; otherwise, <c>false</c>.</returns>
		protected override bool IsHorizontal()
		{
			return ListView.IsHorizontal();
		}

		/// <summary>
		/// Recalculate the pages count.
		/// </summary>
		protected override void RecalculatePages()
		{
			SetScrollRectMaxDrag();

			var per_block = ListView.GetItemsPerBlock() * PerPage;

			Pages = (per_block == 0) ? 0 : Mathf.CeilToInt(((float)ListView.GetItemsCount()) / per_block);

			UpdateLastPageMargin();

			if (currentPage >= Pages)
			{
				GoToPage(Pages - 1);
			}
		}

		/// <summary>
		/// Gets the page position.
		/// </summary>
		/// <returns>The page position.</returns>
		/// <param name="page">Page.</param>
		protected override float Page2Position(int page)
		{
			var index = page * ListView.GetItemsPerBlock() * PerPage;
			switch (ForcedPosition)
			{
				case PaginatorPagePosition.None:
				case PaginatorPagePosition.OnStart:
					return ListView.GetItemPosition(index);
				case PaginatorPagePosition.OnCenter:
					return ListView.GetItemPositionMiddle(index);
				case PaginatorPagePosition.OnEnd:
					return ListView.GetItemPositionBottom(index);
				default:
					throw new NotSupportedException("Unknown forced position: " + ForcedPosition);
			}
		}

		/// <summary>
		/// Set ScrollRect content position.
		/// </summary>
		/// <returns>Position.</returns>
		protected override float GetPosition()
		{
			return ListView.GetScrollPosition();
		}

		/// <summary>
		/// Set ScrollRect content position.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="isHorizontal">Is horizontal direction.</param>
		protected override void SetPosition(float position, bool isHorizontal)
		{
			ListView.ScrollToPosition(position);
		}

		/// <summary>
		/// Start animation.
		/// </summary>
		/// <param name="target">Target position.</param>
		protected override void StartAnimation(float target)
		{
			ListView.ScrollToPositionAnimated(target);
		}

		/// <inheritdoc/>
		protected override bool IsPrevAvailable(int page)
		{
			return (Pages == 0)
				? false
				: (page != 0) || ListView.LoopedListAvailable;
		}

		/// <inheritdoc/>
		protected override bool IsNextAvailable(int page)
		{
			return (Pages == 0)
				? false
				: (page != (Pages - 1)) || ListView.LoopedListAvailable;
		}

		/// <inheritdoc/>
		public override void Next()
		{
			if (CurrentPage == (Pages - 1))
			{
				CurrentPage = 0;
			}
			else
			{
				CurrentPage += 1;
			}
		}

		/// <inheritdoc/>
		public override void Prev()
		{
			if (CurrentPage == 0)
			{
				CurrentPage = Pages - 1;
			}
			else
			{
				CurrentPage -= 1;
			}
		}
	}
}