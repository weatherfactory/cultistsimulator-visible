namespace EasyLayoutNS
{
	using UnityEngine;

	/// <summary>
	/// Compact layout group.
	/// </summary>
	public class EasyLayoutCompact : EasyLayoutCompactOrGrid
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutCompact"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public EasyLayoutCompact(EasyLayout layout)
				: base(layout)
		{
		}

		/// <summary>
		/// Group the specified elements.
		/// </summary>
		protected override void Group()
		{
			if (ElementsGroup.Count == 0)
			{
				return;
			}

			if (Layout.IsHorizontal)
			{
				GroupHorizontal();
			}
			else
			{
				GroupVertical();
			}

			var rows = ElementsGroup.Rows;
			var columns = ElementsGroup.Columns;

			if ((Layout.CompactConstraint == CompactConstraints.MaxRowCount) && (rows > Layout.ConstraintCount))
			{
				ElementsGroup.Clear();
				GroupByRows();
			}
			else if ((Layout.CompactConstraint == CompactConstraints.MaxColumnCount) && (columns > Layout.ConstraintCount))
			{
				ElementsGroup.Clear();
				GroupByColumns();
			}

			if (!Layout.TopToBottom)
			{
				ElementsGroup.BottomToTop();
			}

			if (Layout.RightToLeft)
			{
				ElementsGroup.RightToLeft();
			}
		}

		void GroupHorizontal()
		{
			var base_length = Layout.MainAxisSize;
			var length = base_length;
			var spacing = Layout.Spacing.x;

			int row = 0;
			int column = 0;
			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				var element = ElementsGroup[i];
				if (column == 0)
				{
					length -= element.AxisSize;
				}
				else if (length >= (element.AxisSize + spacing))
				{
					length -= element.AxisSize + spacing;
				}
				else
				{
					length = base_length - element.AxisSize;

					row++;
					column = 0;
				}

				ElementsGroup.SetPosition(i, row, column);
				column++;
			}
		}

		void GroupVertical()
		{
			var base_length = Layout.MainAxisSize;
			var length = base_length;
			var spacing = Layout.Spacing.y;

			int row = 0;
			int column = 0;

			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				var element = ElementsGroup[i];
				if (row == 0)
				{
					length -= element.AxisSize;
				}
				else if (length >= (element.AxisSize + spacing))
				{
					length -= element.AxisSize + spacing;
				}
				else
				{
					length = base_length - element.AxisSize;

					column++;
					row = 0;
				}

				ElementsGroup.SetPosition(i, row, column);
				row++;
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
				if (Layout.IsHorizontal)
				{
					if (Layout.ChildrenWidth == ChildrenSize.FitContainer)
					{
						ResizeWidthToFit(false);
					}
					else if (Layout.ChildrenWidth == ChildrenSize.SetPreferredAndFitContainer)
					{
						ResizeWidthToFit(true);
					}
					else if (Layout.ChildrenWidth == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkWidthToFit();
					}

					if (Layout.ChildrenHeight == ChildrenSize.FitContainer)
					{
						ResizeRowHeightToFit(false);
					}
					else if (Layout.ChildrenHeight == ChildrenSize.SetPreferredAndFitContainer)
					{
						ResizeRowHeightToFit(true);
					}
					else if (Layout.ChildrenHeight == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkRowHeightToFit();
					}
				}
				else
				{
					if (Layout.ChildrenWidth == ChildrenSize.FitContainer)
					{
						ResizeColumnWidthToFit(false);
					}
					else if (Layout.ChildrenWidth == ChildrenSize.SetPreferredAndFitContainer)
					{
						ResizeColumnWidthToFit(true);
					}
					else if (Layout.ChildrenWidth == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkColumnWidthToFit();
					}

					if (Layout.ChildrenHeight == ChildrenSize.FitContainer)
					{
						ResizeHeightToFit(false);
					}
					else if (Layout.ChildrenHeight == ChildrenSize.SetPreferredAndFitContainer)
					{
						ResizeHeightToFit(true);
					}
					else if (Layout.ChildrenHeight == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkHeightToFit();
					}
				}
			}
		}

		void ResizeHeightToFit(bool increaseOnly)
		{
			var height = Layout.InternalSize.y;
			for (int column = 0; column < ElementsGroup.Columns; column++)
			{
				ResizeToFit(height, ElementsGroup.GetColumn(column), Layout.Spacing.y, RectTransform.Axis.Vertical, increaseOnly);
			}
		}

		void ShrinkHeightToFit()
		{
			var height = Layout.InternalSize.y;
			for (int column = 0; column < ElementsGroup.Columns; column++)
			{
				ShrinkToFit(height, ElementsGroup.GetColumn(column), Layout.Spacing.y, RectTransform.Axis.Vertical);
			}
		}

		/// <summary>
		/// Calculate size of the group.
		/// </summary>
		/// <param name="isHorizontal">ElementsGroup are in horizontal order?</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="padding">Padding,</param>
		/// <returns>Size.</returns>
		protected override Vector2 CalculateGroupSize(bool isHorizontal, Vector2 spacing, Vector2 padding)
		{
			return ElementsGroup.Size(spacing, padding);
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
			return new Vector2(
				emptyWidth * RowAligns[(int)Layout.RowAlign],
				(cellMaxSize.y - element.Height) * InnerAligns[(int)Layout.InnerAlign]);
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
			return new Vector2(
				(cellMaxSize.x - element.Width) * InnerAligns[(int)Layout.InnerAlign],
				emptyHeight * RowAligns[(int)Layout.RowAlign]);
		}
	}
}