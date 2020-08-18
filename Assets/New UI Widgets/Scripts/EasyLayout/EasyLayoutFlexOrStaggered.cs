namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Base class for the flex and staggered layout groups.
	/// </summary>
	public abstract class EasyLayoutFlexOrStaggered : EasyLayoutBaseType
	{
		/// <summary>
		/// Axis data.
		/// </summary>
		protected struct AxisData
		{
			/// <summary>
			/// Offset.
			/// </summary>
			public float Offset;

			/// <summary>
			/// Spacing.
			/// </summary>
			public float Spacing;

			/// <summary>
			/// Serves as a hash function for a object.
			/// </summary>
			/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
			public override int GetHashCode()
			{
				return Mathf.RoundToInt(Offset) ^ Mathf.RoundToInt(Spacing);
			}

			/// <summary>
			/// Determines whether the specified System.Object is equal to the current axis data.
			/// </summary>
			/// <param name="obj">The System.Object to compare with the current axis data.</param>
			/// <returns><c>true</c> if the specified System.Object is equal to the current axis data; otherwise, <c>false</c>.</returns>
			public override bool Equals(object obj)
			{
				if (!(obj is AxisData))
				{
					return false;
				}

				return Equals((AxisData)obj);
			}

			/// <summary>
			/// Determines whether the specified axis data is equal to the current axis data.
			/// </summary>
			/// <param name="other">The axis data to compare with the current axis data.</param>
			/// <returns><c>true</c> if the specified axis data is equal to the current axis data; otherwise, <c>false</c>.</returns>
			public bool Equals(AxisData other)
			{
				if (Offset != other.Offset)
				{
					return false;
				}

				return Spacing == other.Spacing;
			}

			/// <summary>
			/// Compare axis data.
			/// </summary>
			/// <param name="obj1">First axis data.</param>
			/// <param name="obj2">Second axis data.</param>
			/// <returns>True if data are equals; otherwise false.</returns>
			public static bool operator ==(AxisData obj1, AxisData obj2)
			{
				return obj1.Equals(obj2);
			}

			/// <summary>
			/// Compare axis data.
			/// </summary>
			/// <param name="obj1">First axis data.</param>
			/// <param name="obj2">Seconds axis data.</param>
			/// <returns>True if data are not equals; otherwise false.</returns>
			public static bool operator !=(AxisData obj1, AxisData obj2)
			{
				return !obj1.Equals(obj2);
			}
		}

		/// <summary>
		/// Sizes of blocks at the sub axis.
		/// </summary>
		protected List<float> SubAxisSizes = new List<float>();

		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutFlexOrStaggered"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		protected EasyLayoutFlexOrStaggered(EasyLayout layout)
				: base(layout)
		{
		}

		/// <summary>
		/// Calculate sizes of the elements.
		/// </summary>
		protected override void CalculateSizes()
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

		void ResizeColumnWidthToFit()
		{
			ResizeToFit(Layout.InternalSize.x, ElementsGroup, Layout.Spacing.x, RectTransform.Axis.Horizontal);
		}

		void ShrinkColumnWidthToFit()
		{
			ShrinkToFit(Layout.InternalSize.x, ElementsGroup, Layout.Spacing.x, RectTransform.Axis.Horizontal);
		}

		void ResizeHeightToFit()
		{
			var height = Layout.InternalSize.y;
			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				ResizeToFit(height, ElementsGroup[i], Layout.Spacing.y, RectTransform.Axis.Vertical);
			}
		}

		void ShrinkHeightToFit()
		{
			var height = Layout.InternalSize.y;
			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				ShrinkToFit(height, ElementsGroup[i], Layout.Spacing.y, RectTransform.Axis.Vertical);
			}
		}

		/// <summary>
		/// Convert input to match direction.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <returns>Converted input.</returns>
		protected Vector2 ByAxis(Vector2 input)
		{
			return Layout.IsHorizontal ? input : new Vector2(input.y, input.x);
		}

		/// <summary>
		/// Calculate group size.
		/// </summary>
		/// <returns>Size.</returns>
		protected override Vector2 CalculateGroupSize()
		{
			var spacing = ByAxis(Layout.Spacing);
			var padding = ByAxis(new Vector2(Layout.PaddingInner.Horizontal, Layout.PaddingInner.Vertical));

			return CalculateGroupSize(Layout.IsHorizontal, spacing, padding);
		}

		/// <summary>
		/// Calculate positions of the elements.
		/// </summary>
		/// <param name="size">Size.</param>
		protected override void CalculatePositions(Vector2 size)
		{
			var sub_axis = SubAxisData();
			var axis_direction = Layout.IsHorizontal ? -1f : 1f;

			var sub_position = sub_axis.Offset * axis_direction;
			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				CalculatePositions(i, sub_position, SubAxisSizes[i]);

				sub_position += (SubAxisSizes[i] + sub_axis.Spacing) * axis_direction;
			}
		}

		/// <summary>
		/// Calculate sizes of the blocks at sub axis.
		/// </summary>
		protected void CalculateSubAxisSizes()
		{
			SubAxisSizes.Clear();

			for (int i = 0; i < ElementsGroup.Count; i++)
			{
				var block = ElementsGroup[i];
				var block_size = 0f;
				for (int j = 0; j < block.Count; j++)
				{
					block_size = Mathf.Max(block_size, block[j].SubAxisSize);
				}

				SubAxisSizes.Add(block_size);
			}
		}

		/// <summary>
		/// Get axis data.
		/// </summary>
		/// <param name="isMainAxis">Is main axis?</param>
		/// <param name="elements">Elements count.</param>
		/// <param name="size">Total size of the elements.</param>
		/// <returns>Axis data</returns>
		protected virtual AxisData GetAxisData(bool isMainAxis, int elements, float size)
		{
			var axis = new AxisData()
			{
				Offset = BaseOffset(isMainAxis),
				Spacing = isMainAxis ? ByAxis(Layout.Spacing).x : ByAxis(Layout.Spacing).y,
			};

			return axis;
		}

		/// <summary>
		/// Calculate base offset.
		/// </summary>
		/// <param name="isMainAxis">Is main axis?</param>
		/// <returns>Base offset.</returns>
		protected float BaseOffset(bool isMainAxis)
		{
			var rectTransform = Layout.transform as RectTransform;

			return Layout.IsHorizontal == isMainAxis
				? (rectTransform.rect.width * (-rectTransform.pivot.x)) + Layout.GetMarginLeft()
				: (rectTransform.rect.height * (rectTransform.pivot.y - 1f)) + Layout.GetMarginTop();
		}

		/// <summary>
		/// Get sub axis data.
		/// </summary>
		/// <returns>Sub axis data.</returns>
		protected virtual AxisData SubAxisData()
		{
			CalculateSubAxisSizes();

			return GetAxisData(false, SubAxisSizes.Count, Sum(SubAxisSizes));
		}

		/// <summary>
		/// Get main axis data for the block with the specified index.
		/// </summary>
		/// <param name="blockIndex">Block index.</param>
		/// <returns>Main axis data.</returns>
		protected virtual AxisData MainAxisData(int blockIndex)
		{
			var block = ElementsGroup[blockIndex];
			var size = 0f;
			for (int i = 0; i < block.Count; i++)
			{
				size += block[i].AxisSize;
			}

			return GetAxisData(true, block.Count, size);
		}

		/// <summary>
		/// Calculate positions.
		/// </summary>
		/// <param name="blockIndex">Index of the block to calculate positions.</param>
		/// <param name="subAxisOffset">Offset on the sub axis.</param>
		/// <param name="maxSubSize">Maximum size of the sub axis.</param>
		protected void CalculatePositions(int blockIndex, float subAxisOffset, float maxSubSize)
		{
			var block = ElementsGroup[blockIndex];
			var axis_direction = Layout.IsHorizontal ? 1f : -1f;
			var axis = MainAxisData(blockIndex);

			var position = new Vector2(axis.Offset, subAxisOffset);
			var sub_offset_rate = GetItemsAlignRate();
			for (int i = 0; i < block.Count; i++)
			{
				var sub_axis = subAxisOffset - ((maxSubSize - block[i].SubAxisSize) * sub_offset_rate * axis_direction);

				block[i].PositionTopLeft = ByAxis(new Vector2(position.x * axis_direction, sub_axis));
				position.x += block[i].AxisSize + axis.Spacing;
			}
		}

		/// <summary>
		/// Get align rate for the items.
		/// </summary>
		/// <returns>Align rate.</returns>
		protected virtual float GetItemsAlignRate()
		{
			return 0f;
		}
	}
}