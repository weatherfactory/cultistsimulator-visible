namespace EasyLayoutNS
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Base class for EasyLayout groups.
	/// </summary>
	public abstract class EasyLayoutBaseType
	{
		/// <summary>
		/// Layout.
		/// </summary>
		protected EasyLayout Layout;

		/// <summary>
		/// Elements group.
		/// </summary>
		protected LayoutElementsGroup ElementsGroup = new LayoutElementsGroup();

		/// <summary>
		/// Group position.
		/// </summary>
		protected static readonly List<Vector2> GroupPositions = new List<Vector2>()
		{
			new Vector2(0.0f, 1.0f), // Anchors.UpperLeft
			new Vector2(0.5f, 1.0f), // Anchors.UpperCenter
			new Vector2(1.0f, 1.0f), // Anchors.UpperRight

			new Vector2(0.0f, 0.5f), // Anchors.MiddleLeft
			new Vector2(0.5f, 0.5f), // Anchors.MiddleCenter
			new Vector2(1.0f, 0.5f), // Anchors.MiddleRight

			new Vector2(0.0f, 0.0f), // Anchors.LowerLeft
			new Vector2(0.5f, 0.0f), // Anchors.LowerCenter
			new Vector2(1.0f, 0.0f), // Anchors.LowerRight
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutBaseType"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		protected EasyLayoutBaseType(EasyLayout layout)
		{
			Layout = layout;
		}

		/// <summary>
		/// Perform layout.
		/// </summary>
		/// <param name="elements">Elements.</param>
		/// <param name="setPositions">Set elements positions.</param>
		/// <returns>Size of the group.</returns>
		public Vector2 PerformLayout(List<LayoutElementInfo> elements, bool setPositions)
		{
			ElementsGroup.SetElements(elements);

			SetInitialSizes();
			Group();
			ElementsGroup.Sort();

			CalculateSizes();
			var size = CalculateGroupSize();
			CalculatePositions(size);
			SetSizes();

			if (setPositions)
			{
				SetPositions();
			}

			return size;
		}

		/// <summary>
		/// Get target position in the group.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <returns>Position.</returns>
		public virtual EasyLayoutPosition GetElementPosition(RectTransform target)
		{
			return ElementsGroup.GetElementPosition(target);
		}

		/// <summary>
		/// Group elements.
		/// </summary>
		protected abstract void Group();

		/// <summary>
		/// Calculate sizes of the elements.
		/// </summary>
		protected abstract void CalculateSizes();

		/// <summary>
		/// Calculate positions of the elements.
		/// </summary>
		/// <param name="size">Size.</param>
		protected abstract void CalculatePositions(Vector2 size);

		/// <summary>
		/// Calculate group size.
		/// </summary>
		/// <returns>Size.</returns>
		protected abstract Vector2 CalculateGroupSize();

		readonly List<float> mainAxisSizes = new List<float>();

		/// <summary>
		/// Calculate size of the group.
		/// </summary>
		/// <param name="isHorizontal">ElementsGroup are in horizontal order?</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="padding">Padding,</param>
		/// <returns>Size.</returns>
		protected virtual Vector2 CalculateGroupSize(bool isHorizontal, Vector2 spacing, Vector2 padding)
		{
			var per_block = isHorizontal ? ElementsGroup.Rows : ElementsGroup.Columns;
			var sub_axis_size = ((per_block - 1) * spacing.y) + padding.y;
			for (int i = 0; i < per_block; i++)
			{
				var block = isHorizontal ? ElementsGroup.GetRow(i) : ElementsGroup.GetColumn(i);
				var block_sub_size = 0f;
				for (var j = 0; j < block.Count; j++)
				{
					if (mainAxisSizes.Count == j)
					{
						mainAxisSizes.Add(0);
					}

					mainAxisSizes[j] = Mathf.Max(mainAxisSizes[j], isHorizontal ? block[j].Width : block[j].Height);
					block_sub_size = Mathf.Max(block_sub_size, isHorizontal ? block[j].Height : block[j].Width);
				}

				sub_axis_size += block_sub_size;
			}

			var main_axis_size = Sum(mainAxisSizes) + ((mainAxisSizes.Count - 1) * spacing.x) + padding.x;
			mainAxisSizes.Clear();

			return new Vector2(main_axis_size, sub_axis_size);
		}

		/// <summary>
		/// Set elements sizes.
		/// </summary>
		protected void SetSizes()
		{
			foreach (var element in ElementsGroup.Elements)
			{
				Layout.SetElementSize(element);
			}
		}

		/// <summary>
		/// Set elements positions.
		/// </summary>
		protected virtual void SetPositions()
		{
			foreach (var element in ElementsGroup.Elements)
			{
				if (element.IsPositionChanged)
				{
					element.Rect.localPosition = element.PositionPivot;
				}
			}
		}

		/// <summary>
		/// Sum values of the list.
		/// </summary>
		/// <param name="list">List.</param>
		/// <returns>Sum.</returns>
		protected static float Sum(List<float> list)
		{
			var result = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				result += list[i];
			}

			return result;
		}

		#region Group

		/// <summary>
		/// Reverse list.
		/// </summary>
		/// <param name="list">List.</param>
		protected static void ReverseList(List<LayoutElementInfo> list)
		{
			list.Reverse();
		}

		/// <summary>
		/// Group elements by columns in the vertical order.
		/// </summary>
		/// <param name="maxColumns">Maximum columns count.</param>
		protected void GroupByColumnsVertical(int maxColumns)
		{
			int i = 0;
			for (int column = 0; column < maxColumns; column++)
			{
				int max_rows = Mathf.CeilToInt(((float)(ElementsGroup.Count - i)) / (maxColumns - column));
				for (int row = 0; row < max_rows; row++)
				{
					ElementsGroup.SetPosition(i, row, column);

					i += 1;
				}
			}
		}

		/// <summary>
		/// Group elements by columns in the horizontal order.
		/// </summary>
		/// <param name="maxColumns">Maximum columns count.</param>
		protected void GroupByColumnsHorizontal(int maxColumns)
		{
			int row = 0;

			for (int i = 0; i < ElementsGroup.Count; i += maxColumns)
			{
				int column = 0;
				var end = Mathf.Min(i + maxColumns, ElementsGroup.Count);
				for (int j = i; j < end; j++)
				{
					ElementsGroup.SetPosition(j, row, column);
					column += 1;
				}

				row += 1;
			}
		}

		/// <summary>
		/// Group elements by rows in the vertical order.
		/// </summary>
		/// <param name="maxRows">Maximum rows count.</param>
		protected void GroupByRowsVertical(int maxRows)
		{
			int column = 0;
			for (int i = 0; i < ElementsGroup.Count; i += maxRows)
			{
				int row = 0;
				var end = Mathf.Min(i + maxRows, ElementsGroup.Count);
				for (int j = i; j < end; j++)
				{
					ElementsGroup.SetPosition(j, row, column);
					row += 1;
				}

				column += 1;
			}
		}

		/// <summary>
		/// Group elements by rows in the horizontal order.
		/// </summary>
		/// <param name="maxRows">Maximum rows count.</param>
		protected void GroupByRowsHorizontal(int maxRows)
		{
			int i = 0;
			for (int row = 0; row < maxRows; row++)
			{
				int max_columns = Mathf.CeilToInt((float)(ElementsGroup.Count - i) / (maxRows - row));
				for (int column = 0; column < max_columns; column++)
				{
					ElementsGroup.SetPosition(i, row, column);
					i += 1;
				}
			}
		}

		/// <summary>
		/// Group the specified uiElements by columns.
		/// </summary>
		protected void GroupByColumns()
		{
			if (Layout.IsHorizontal)
			{
				GroupByColumnsHorizontal(Layout.ConstraintCount);
			}
			else
			{
				GroupByColumnsVertical(Layout.ConstraintCount);
			}
		}

		/// <summary>
		/// Group the specified uiElements by rows.
		/// </summary>
		protected void GroupByRows()
		{
			if (Layout.IsHorizontal)
			{
				GroupByRowsHorizontal(Layout.ConstraintCount);
			}
			else
			{
				GroupByRowsVertical(Layout.ConstraintCount);
			}
		}
		#endregion

		#region Sizes

		/// <summary>
		/// Resize elements.
		/// </summary>
		protected virtual void SetInitialSizes()
		{
			if (Layout.ChildrenWidth == ChildrenSize.DoNothing && Layout.ChildrenHeight == ChildrenSize.DoNothing)
			{
				return;
			}

			if (ElementsGroup.Count == 0)
			{
				return;
			}

			var max_size = FindMaxPreferredSize();

			foreach (var element in ElementsGroup.Elements)
			{
				SetInitialSize(element, max_size);
			}
		}

		Vector2 FindMaxPreferredSize()
		{
			var max_size = new Vector2(-1f, -1f);

			foreach (var element in ElementsGroup.Elements)
			{
				max_size.x = Mathf.Max(max_size.x, element.PreferredWidth);
				max_size.y = Mathf.Max(max_size.y, element.PreferredHeight);
			}

			if (Layout.ChildrenWidth != ChildrenSize.SetMaxFromPreferred)
			{
				max_size.x = -1f;
			}

			if (Layout.ChildrenHeight != ChildrenSize.SetMaxFromPreferred)
			{
				max_size.y = -1f;
			}

			return max_size;
		}

		void SetInitialSize(LayoutElementInfo element, Vector2 max_size)
		{
			if (Layout.ChildrenWidth != ChildrenSize.DoNothing)
			{
				element.NewWidth = (max_size.x != -1f) ? max_size.x : element.PreferredWidth;
			}

			if (Layout.ChildrenHeight != ChildrenSize.DoNothing)
			{
				element.NewHeight = (max_size.y != -1f) ? max_size.y : element.PreferredHeight;
			}
		}

		/// <summary>
		/// Resize elements width to fit.
		/// </summary>
		/// <param name="increaseOnly">Size can be only increased.</param>
		protected void ResizeWidthToFit(bool increaseOnly)
		{
			var width = Layout.InternalSize.x;
			for (int row = 0; row < ElementsGroup.Rows; row++)
			{
				ResizeToFit(width, ElementsGroup.GetRow(row), Layout.Spacing.x, RectTransform.Axis.Horizontal, increaseOnly);
			}
		}

		/// <summary>
		/// Resize specified elements to fit.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="elements">Elements.</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="axis">Axis to fit.</param>
		/// <param name="increaseOnly">Size can be only increased.</param>
		protected static void ResizeToFit(float size, List<LayoutElementInfo> elements, float spacing, RectTransform.Axis axis, bool increaseOnly)
		{
			var sizes = axis == RectTransform.Axis.Horizontal ? SizesInfo.GetWidths(elements) : SizesInfo.GetHeights(elements);
			var free_space = size - sizes.TotalPreferred - ((elements.Count - 1) * spacing);

			if (increaseOnly)
			{
				free_space = Mathf.Max(0f, free_space);
				size = Mathf.Max(0f, size);
				sizes.TotalMin = sizes.TotalPreferred;
			}

			var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

			var minPrefLerp = 1f;
			if (sizes.TotalMin != sizes.TotalPreferred)
			{
				minPrefLerp = Mathf.Clamp01((size - sizes.TotalMin - ((elements.Count - 1) * spacing)) / (sizes.TotalPreferred - sizes.TotalMin));
			}

			for (int i = 0; i < elements.Count; i++)
			{
				var element_size = Mathf.Lerp(sizes.Sizes[i].Min, sizes.Sizes[i].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[i].Flexible);
				elements[i].SetSize(axis, element_size);
			}
		}

		/// <summary>
		/// Shrink elements width to fit.
		/// </summary>
		protected void ShrinkWidthToFit()
		{
			var width = Layout.InternalSize.x;
			for (int row = 0; row < ElementsGroup.Rows; row++)
			{
				ShrinkToFit(width, ElementsGroup.GetRow(row), Layout.Spacing.x, RectTransform.Axis.Horizontal);
			}
		}

		/// <summary>
		/// Resize row height to fit.
		/// </summary>
		/// <param name="increaseOnly">Size can be only increased.</param>
		protected void ResizeRowHeightToFit(bool increaseOnly)
		{
			ResizeToFit(Layout.InternalSize.y, ElementsGroup, Layout.Spacing.y, RectTransform.Axis.Vertical, increaseOnly);
		}

		/// <summary>
		/// Shrink row height to fit.
		/// </summary>
		protected void ShrinkRowHeightToFit()
		{
			ShrinkToFit(Layout.InternalSize.y, ElementsGroup, Layout.Spacing.y, RectTransform.Axis.Vertical);
		}

		/// <summary>
		/// Shrink specified elements size to fit.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="elements">Elements.</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="axis">Axis to fit.</param>
		protected static void ShrinkToFit(float size, List<LayoutElementInfo> elements, float spacing, RectTransform.Axis axis)
		{
			var sizes = axis == RectTransform.Axis.Horizontal ? SizesInfo.GetWidths(elements) : SizesInfo.GetHeights(elements);

			float free_space = size - sizes.TotalPreferred - ((elements.Count - 1) * spacing);
			if (free_space > 0f)
			{
				return;
			}

			var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

			var minPrefLerp = 0f;
			if (sizes.TotalMin != sizes.TotalPreferred)
			{
				minPrefLerp = Mathf.Clamp01((size - sizes.TotalMin - ((elements.Count - 1) * spacing)) / (sizes.TotalPreferred - sizes.TotalMin));
			}

			for (int i = 0; i < elements.Count; i++)
			{
				var element_size = Mathf.Lerp(sizes.Sizes[i].Min, sizes.Sizes[i].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[i].Flexible);
				elements[i].SetSize(axis, element_size);
			}
		}

		/// <summary>
		/// Resize specified elements to fit.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="group">Group.</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="axis">Axis to fit.</param>
		/// <param name="increaseOnly">Size can be only increased.</param>
		protected static void ResizeToFit(float size, LayoutElementsGroup group, float spacing, RectTransform.Axis axis, bool increaseOnly)
		{
			var is_horizontal = axis == RectTransform.Axis.Horizontal;
			var sizes = is_horizontal ? SizesInfo.GetWidths(group) : SizesInfo.GetHeights(group);
			var n = is_horizontal ? group.Columns : group.Rows;
			var free_space = size - sizes.TotalPreferred - ((n - 1) * spacing);

			if (increaseOnly)
			{
				free_space = Mathf.Max(0f, free_space);
				size = Mathf.Max(0f, size);
				sizes.TotalMin = sizes.TotalPreferred;
			}

			var minPrefLerp = 1f;
			if (sizes.TotalMin != sizes.TotalPreferred)
			{
				minPrefLerp = Mathf.Clamp01((size - sizes.TotalMin - ((group.Rows - 1) * spacing)) / (sizes.TotalPreferred - sizes.TotalMin));
			}

			if (is_horizontal)
			{
				var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

				for (int column = 0; column < group.Columns; column++)
				{
					var element_size = Mathf.Lerp(sizes.Sizes[column].Min, sizes.Sizes[column].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[column].Flexible);

					foreach (var element in group.GetColumn(column))
					{
						element.SetSize(axis, element_size);
					}
				}
			}
			else
			{
				var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

				for (int rows = 0; rows < group.Rows; rows++)
				{
					var element_size = Mathf.Lerp(sizes.Sizes[rows].Min, sizes.Sizes[rows].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[rows].Flexible);
					foreach (var element in group.GetRow(rows))
					{
						element.SetSize(axis, element_size);
					}
				}
			}
		}

		/// <summary>
		/// Shrink specified elements to fit.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="group">Elements.</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="axis">Axis to fit.</param>
		protected static void ShrinkToFit(float size, LayoutElementsGroup group, float spacing, RectTransform.Axis axis)
		{
			var is_horizontal = axis == RectTransform.Axis.Horizontal;
			var sizes = is_horizontal ? SizesInfo.GetWidths(group) : SizesInfo.GetHeights(group);
			var n = is_horizontal ? group.Columns : group.Rows;

			var free_space = size - sizes.TotalPreferred - ((n - 1) * spacing);
			if (free_space > 0f)
			{
				return;
			}

			var minPrefLerp = 0f;
			if (sizes.TotalMin != sizes.TotalPreferred)
			{
				minPrefLerp = Mathf.Clamp01((size - sizes.TotalMin - ((group.Rows - 1) * spacing)) / (sizes.TotalPreferred - sizes.TotalMin));
			}

			if (is_horizontal)
			{
				var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

				for (int column = 0; column < group.Columns; column++)
				{
					var element_size = Mathf.Lerp(sizes.Sizes[column].Min, sizes.Sizes[column].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[column].Flexible);

					foreach (var element in group.GetColumn(column))
					{
						element.SetSize(axis, element_size);
					}
				}
			}
			else
			{
				var per_flexible = free_space > 0f ? free_space / sizes.TotalFlexible : 0f;

				for (int rows = 0; rows < group.Rows; rows++)
				{
					var element_size = Mathf.Lerp(sizes.Sizes[rows].Min, sizes.Sizes[rows].Preferred, minPrefLerp) + (per_flexible * sizes.Sizes[rows].Flexible);

					foreach (var element in group.GetRow(rows))
					{
						element.SetSize(axis, element_size);
					}
				}
			}
		}
		#endregion
	}
}