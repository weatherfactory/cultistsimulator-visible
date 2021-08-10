namespace EasyLayoutNS
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Base class for the compact and grid layout groups.
	/// </summary>
	public abstract class EasyLayoutCompactOrGrid : EasyLayoutBaseType
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutCompactOrGrid"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		protected EasyLayoutCompactOrGrid(EasyLayout layout)
				: base(layout)
		{
		}

		#region Position

		/// <summary>
		/// Row aligns.
		/// </summary>
		protected static readonly List<float> RowAligns = new List<float>()
		{
			0.0f, // HorizontalAligns.Left
			0.5f, // HorizontalAligns.Center
			1.0f, // HorizontalAligns.Right
		};

		/// <summary>
		/// Inner aligns.
		/// </summary>
		protected static readonly List<float> InnerAligns = new List<float>()
		{
			0.0f, // InnerAligns.Top
			0.5f, // InnerAligns.Middle
			1.0f, // InnerAligns.Bottom
		};

		readonly List<float> RowsWidths = new List<float>();
		readonly List<float> MaxColumnsWidths = new List<float>();

		readonly List<float> ColumnsHeights = new List<float>();
		readonly List<float> MaxRowsHeights = new List<float>();

		void CalculateRowsWidths()
		{
			RowsWidths.Clear();

			for (int i = 0; i < ElementsGroup.Rows; i++)
			{
				var row = ElementsGroup.GetRow(i);
				var row_width = (row.Count - 1) * Layout.Spacing.x;
				foreach (var element in row)
				{
					row_width += element.Width;
				}

				RowsWidths.Add(row_width);
			}
		}

		void CalculateMaxColumnsWidths()
		{
			MaxColumnsWidths.Clear();

			for (var i = 0; i < ElementsGroup.Rows; i++)
			{
				var row = ElementsGroup.GetRow(i);
				for (var j = 0; j < row.Count; j++)
				{
					if (MaxColumnsWidths.Count == j)
					{
						MaxColumnsWidths.Add(0);
					}

					MaxColumnsWidths[j] = Mathf.Max(MaxColumnsWidths[j], row[j].Width);
				}
			}
		}

		void CalculateColumnsHeights()
		{
			ColumnsHeights.Clear();

			for (int i = 0; i < ElementsGroup.Columns; i++)
			{
				var column = ElementsGroup.GetColumn(i);
				var column_height = (column.Count - 1) * Layout.Spacing.y;
				foreach (var element in column)
				{
					column_height += element.Height;
				}

				ColumnsHeights.Add(column_height);
			}
		}

		void CalculateMaxRowsHeights()
		{
			MaxRowsHeights.Clear();

			for (int i = 0; i < ElementsGroup.Rows; i++)
			{
				var row = ElementsGroup.GetRow(i);
				var row_height = 0f;
				foreach (var element in row)
				{
					row_height = Mathf.Max(row_height, element.Height);
				}

				MaxRowsHeights.Add(row_height);
			}
		}

		static Vector2 GetMaxCellSize(List<LayoutElementInfo> row)
		{
			var x = 0f;
			var y = 0f;
			for (int i = 0; i < row.Count; i++)
			{
				x = Mathf.Max(x, row[i].Width);
				y = Mathf.Max(y, row[i].Height);
			}

			return new Vector2(x, y);
		}

		/// <summary>
		/// Get aligned width.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="maxWidth">Maximum width.</param>
		/// <param name="cellMaxSize">Max size of the cell.</param>
		/// <param name="emptyWidth">Width of the empty space.</param>
		/// <returns>Aligned width.</returns>
		protected abstract Vector2 GetAlignByWidth(LayoutElementInfo element, float maxWidth, Vector2 cellMaxSize, float emptyWidth);

		/// <summary>
		/// Get aligned height.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <param name="maxHeight">Maximum height.</param>
		/// <param name="cellMaxSize">Max size of the cell.</param>
		/// <param name="emptyHeight">Height of the empty space.</param>
		/// <returns>Aligned height.</returns>
		protected abstract Vector2 GetAlignByHeight(LayoutElementInfo element, float maxHeight, Vector2 cellMaxSize, float emptyHeight);

		Vector2 CalculateOffset(Vector2 size)
		{
			var rectTransform = Layout.transform as RectTransform;

			var anchor_position = GroupPositions[(int)Layout.GroupPosition];
			var start_position = new Vector2(
				rectTransform.rect.width * (anchor_position.x - rectTransform.pivot.x),
				rectTransform.rect.height * (anchor_position.y - rectTransform.pivot.y));

			start_position.x -= anchor_position.x * size.x;
			start_position.y += (1 - anchor_position.y) * size.y;

			start_position.x += Layout.GetMarginLeft() * (1 - (anchor_position.x * 2));
			start_position.y += Layout.GetMarginTop() * (1 - (anchor_position.y * 2));

			start_position.x += Layout.PaddingInner.Left;
			start_position.y -= Layout.PaddingInner.Top;

			return start_position;
		}

		/// <summary>
		/// Calculate group size.
		/// </summary>
		/// <returns>Size.</returns>
		protected override Vector2 CalculateGroupSize()
		{
			return CalculateGroupSize(true, Layout.Spacing, new Vector2(Layout.PaddingInner.Horizontal, Layout.PaddingInner.Vertical));
		}

		/// <summary>
		/// Calculate positions of the elements.
		/// </summary>
		/// <param name="size">Size.</param>
		protected override void CalculatePositions(Vector2 size)
		{
			var offset = CalculateOffset(size);

			if (Layout.IsHorizontal)
			{
				CalculatePositionsHorizontal(size, offset);
			}
			else
			{
				CalculatePositionsVertical(size, offset);
			}
		}

		void CalculatePositionsHorizontal(Vector2 size, Vector2 offset)
		{
			var position = offset;

			CalculateRowsWidths();
			CalculateMaxColumnsWidths();

			for (int x = 0; x < ElementsGroup.Rows; x++)
			{
				var row = ElementsGroup.GetRow(x);
				var row_cell_max_size = GetMaxCellSize(row);

				foreach (var element in row)
				{
					var align = GetAlignByWidth(element, MaxColumnsWidths[element.Column], row_cell_max_size, size.x - RowsWidths[x]);

					element.PositionTopLeft = GetElementPosition(position, align);

					position.x += ((Layout.LayoutType == LayoutTypes.Compact) ? element.Width : MaxColumnsWidths[element.Column]) + Layout.Spacing.x;
				}

				position.x = offset.x;
				position.y -= row_cell_max_size.y + Layout.Spacing.y;
			}
		}

		Vector2 CalculatePositionsVertical(Vector2 size, Vector2 offset)
		{
			var position = offset;

			CalculateMaxRowsHeights();
			CalculateColumnsHeights();

			for (int y = 0; y < ElementsGroup.Columns; y++)
			{
				var column = ElementsGroup.GetColumn(y);
				var column_cell_max_size = GetMaxCellSize(column);

				foreach (var element in column)
				{
					var align = GetAlignByHeight(element, MaxRowsHeights[element.Row], column_cell_max_size, size.y - ColumnsHeights[y]);

					element.PositionTopLeft = GetElementPosition(position, align);

					position.y -= ((Layout.LayoutType == LayoutTypes.Compact) ? element.Height : MaxRowsHeights[element.Row]) + Layout.Spacing.y;
				}

				position.y = offset.y;
				position.x += column_cell_max_size.x + Layout.Spacing.x;
			}

			return size;
		}

		/// <summary>
		/// Gets the user interface element position.
		/// </summary>
		/// <returns>The user interface position.</returns>
		/// <param name="position">Position.</param>
		/// <param name="align">Align.</param>
		static Vector2 GetElementPosition(Vector2 position, Vector2 align)
		{
			return new Vector2(
				position.x + align.x,
				position.y - align.y);
		}
		#endregion

		#region Sizes

		/// <summary>
		/// Shrink elements on overflow.
		/// </summary>
		protected void ShrinkOnOverflow()
		{
			if (ElementsGroup.Count == 0)
			{
				return;
			}

			var size = Layout.InternalSize;
			var rows = ElementsGroup.Rows - 1;
			var columns = ElementsGroup.Columns - 1;
			var size_without_spacing = new Vector2(size.x - (Layout.Spacing.x * columns), size.y - (Layout.Spacing.y * rows));

			var ui_size_without_spacing = CalculateGroupSize();
			ui_size_without_spacing.x -= Layout.Spacing.x * columns;
			ui_size_without_spacing.x -= Layout.Spacing.y * rows;

			var scale = GetShrinkScale(size_without_spacing, ui_size_without_spacing);

			foreach (var element in ElementsGroup.Elements)
			{
				element.NewWidth = element.Width * scale;
				element.NewHeight = element.Height * scale;
			}
		}

		static float GetShrinkScale(Vector2 requiredSize, Vector2 currentSize)
		{
			var scale = requiredSize.x / currentSize.x;
			if ((scale > 1) || ((currentSize.y * scale) > requiredSize.y))
			{
				return Mathf.Min(1f, requiredSize.y / currentSize.y);
			}

			return Mathf.Min(1f, scale);
		}

		/// <summary>
		/// Shrink columns width to fit.
		/// </summary>
		protected void ShrinkColumnWidthToFit()
		{
			ShrinkToFit(Layout.InternalSize.x, ElementsGroup, Layout.Spacing.x, RectTransform.Axis.Horizontal);
		}

		/// <summary>
		/// Resize columns width to fit.
		/// </summary>
		/// <param name="increaseOnly">Size can be only increased.</param>
		protected void ResizeColumnWidthToFit(bool increaseOnly)
		{
			ResizeToFit(Layout.InternalSize.x, ElementsGroup, Layout.Spacing.x, RectTransform.Axis.Horizontal, increaseOnly);
		}
		#endregion
	}
}