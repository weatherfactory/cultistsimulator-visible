namespace EasyLayoutNS
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Easy layout grid layout.
	/// </summary>
	public class EasyLayoutGrid : EasyLayoutCompactOrGrid
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutGrid"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public EasyLayoutGrid(EasyLayout layout)
				: base(layout)
		{
		}

		/// <summary>
		/// Gets the max columns count.
		/// </summary>
		/// <returns>The max columns count.</returns>
		/// <param name="maxColumns">Max columns.</param>
		int GetMaxColumnsCount(int maxColumns)
		{
			var base_length = Layout.MainAxisSize;
			var length = base_length;
			var spacing = Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;

			bool min_columns_setted = false;
			int min_columns = maxColumns;
			int current_columns = 0;

			for (int i = 0; i < Elements.Count; i++)
			{
				if (current_columns == maxColumns)
				{
					min_columns_setted = true;
					min_columns = Mathf.Min(min_columns, current_columns);

					current_columns = 1;
					length = base_length - Elements[i].AxisSize;
					continue;
				}

				if (current_columns == 0)
				{
					current_columns = 1;
					length = base_length - Elements[i].AxisSize;
					continue;
				}

				if (length >= (Elements[i].AxisSize + spacing))
				{
					length -= Elements[i].AxisSize + spacing;
					current_columns++;
				}
				else
				{
					min_columns_setted = true;
					min_columns = Mathf.Min(min_columns, current_columns);

					current_columns = 1;
					length = base_length - Elements[i].AxisSize;
				}
			}

			if (!min_columns_setted)
			{
				min_columns = current_columns;
			}

			return min_columns;
		}

		/// <summary>
		/// Group the specified uiElements.
		/// </summary>
		protected override void Group()
		{
			ElementsGroup.Clear();

			if (Layout.GridConstraint == GridConstraints.Flexible)
			{
				GroupFlexible();
			}
			else if (Layout.GridConstraint == GridConstraints.FixedRowCount)
			{
				GroupByRows();
			}
			else if (Layout.GridConstraint == GridConstraints.FixedColumnCount)
			{
				GroupByColumns();
			}

			if (!Layout.TopToBottom)
			{
				ElementsGroup.Reverse();
			}

			if (Layout.RightToLeft)
			{
				ElementsGroup.ForEach(ReverseList);
			}
		}

		/// <summary>
		/// Group the specified uiElements.
		/// </summary>
		void GroupFlexible()
		{
			int max_columns = 999999;
			while (true)
			{
				var new_max_columns = GetMaxColumnsCount(max_columns);

				if ((max_columns == new_max_columns) || (new_max_columns == 1))
				{
					break;
				}

				max_columns = new_max_columns;
			}

			if (Layout.IsHorizontal)
			{
				GroupByColumnsHorizontal(max_columns);
			}
			else
			{
				GroupByRowsVertical(max_columns);
			}
		}

		/// <summary>
		/// Calculate sizes of the elements.
		/// </summary>
		protected override void CalculateSizes()
		{
			if ((Layout.ChildrenWidth == ChildrenSize.ShrinkOnOverflow) && (Layout.ChildrenHeight == ChildrenSize.ShrinkOnOverflow))
			{
				ShrinkOnOverflow();
			}
			else
			{
				if (Layout.ChildrenWidth == ChildrenSize.FitContainer)
				{
					ResizeColumnWidthToFit();
				}
				else if (Layout.ChildrenWidth == ChildrenSize.ShrinkOnOverflow)
				{
					ShrinkColumnWidthToFit();
				}

				if (Layout.ChildrenHeight == ChildrenSize.FitContainer)
				{
					ResizeRowHeightToFit();
				}
				else if (Layout.ChildrenHeight == ChildrenSize.ShrinkOnOverflow)
				{
					ShrinkRowHeightToFit();
				}
			}
		}

		/// <summary>
		/// Get aligned width.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="maxWidth">Maximum width.</param>
		/// <param name="cellMaxSize">Max size of the cell.</param>
		/// <param name="emptyWidth">Width of the empty space.</param>
		/// <returns>Aligned width.</returns>
		protected override Vector2 GetAlignByWidth(LayoutElementInfo element, float maxWidth, Vector2 cellMaxSize, float emptyWidth)
		{
			var cell_align = GroupPositions[(int)Layout.CellAlign];

			return new Vector2(
				(maxWidth - element.Width) * cell_align.x,
				(cellMaxSize.y - element.Height) * (1 - cell_align.y));
		}

		/// <summary>
		/// Get aligned height.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="maxHeight">Maximum height.</param>
		/// <param name="cellMaxSize">Max size of the cell.</param>
		/// <param name="emptyHeight">Height of the empty space.</param>
		/// <returns>Aligned height.</returns>
		protected override Vector2 GetAlignByHeight(LayoutElementInfo element, float maxHeight, Vector2 cellMaxSize, float emptyHeight)
		{
			var cell_align = GroupPositions[(int)Layout.CellAlign];

			return new Vector2(
				(cellMaxSize.x - element.Width) * cell_align.x,
				(maxHeight - element.Height) * (1 - cell_align.y));
		}
	}
}