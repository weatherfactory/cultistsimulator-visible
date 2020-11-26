namespace EasyLayoutNS
{
	using System.Collections.Generic;
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
			ElementsGroup.Clear();

			if (Elements.Count == 0)
			{
				return;
			}

			if (Layout.IsHorizontal)
			{
				GroupHorizontal(ElementsGroup);
			}
			else
			{
				GroupVertical(ElementsGroup);
			}

			var rows = ElementsGroup.Count;
			var columns = EasyLayoutUtilities.MaxCount(ElementsGroup);

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
				ElementsGroup.Reverse();
			}

			if (Layout.RightToLeft)
			{
				ElementsGroup.ForEach(ReverseList);
			}
		}

		void GroupHorizontal(List<List<LayoutElementInfo>> resultedGroup)
		{
			var base_length = Layout.MainAxisSize;
			var length = base_length;
			var spacing = Layout.Spacing.x;

			var row = new List<LayoutElementInfo>();
			resultedGroup.Add(row);

			foreach (var element in Elements)
			{
				if (row.Count == 0)
				{
					length -= element.AxisSize;
					row.Add(element);
					continue;
				}

				if (length >= (element.AxisSize + spacing))
				{
					length -= element.AxisSize + spacing;
					row.Add(element);
				}
				else
				{
					length = base_length - element.AxisSize;

					row = new List<LayoutElementInfo>();
					resultedGroup.Add(row);

					row.Add(element);
				}
			}
		}

		void GroupVertical(List<List<LayoutElementInfo>> group)
		{
			var base_length = Layout.MainAxisSize;
			var length = base_length;

			var spacing = Layout.Spacing.y;
			var block = 0;
			group.Add(new List<LayoutElementInfo>());
			foreach (var element in Elements)
			{
				if (group[block].Count == 0)
				{
					length -= element.AxisSize;
					group[block].Add(element);
					continue;
				}

				if (length >= (element.AxisSize + spacing))
				{
					length -= element.AxisSize + spacing;

					block += 1;
					if (group.Count == block)
					{
						group.Add(new List<LayoutElementInfo>());
					}

					group[block].Add(element);
				}
				else
				{
					length = base_length - element.AxisSize;
					group[0].Add(element);
					block = 0;
				}
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
						ResizeWidthToFit();
					}
					else if (Layout.ChildrenWidth == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkWidthToFit();
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
						ResizeHeightToFit();
					}
					else if (Layout.ChildrenHeight == ChildrenSize.ShrinkOnOverflow)
					{
						ShrinkHeightToFit();
					}
				}
			}
		}

		void ResizeHeightToFit()
		{
			var height = Layout.InternalSize.y;
			var transposed_group = EasyLayoutUtilities.Transpose(ElementsGroup);
			for (int i = 0; i < transposed_group.Count; i++)
			{
				ResizeToFit(height, transposed_group[i], Layout.Spacing.y, RectTransform.Axis.Vertical);
			}
		}

		void ShrinkHeightToFit()
		{
			var height = Layout.InternalSize.y;
			var transposed_group = EasyLayoutUtilities.Transpose(ElementsGroup);
			for (int i = 0; i < transposed_group.Count; i++)
			{
				ShrinkToFit(height, transposed_group[i], Layout.Spacing.y, RectTransform.Axis.Vertical);
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