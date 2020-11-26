namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Easy layout staggered layout.
	/// </summary>
	public class EasyLayoutStaggered : EasyLayoutFlexOrStaggered
	{
		readonly List<float> BlocksSizes = new List<float>();

		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutStaggered"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public EasyLayoutStaggered(EasyLayout layout)
				: base(layout)
		{
		}

		int GetBlockSizes()
		{
			if (Elements.Count == 0)
			{
				return 0;
			}

			if (Layout.StaggeredSettings.FixedBlocksCount)
			{
				return Mathf.Max(1, Layout.StaggeredSettings.BlocksCount);
			}

			var blocks = 1;

			var size = Layout.SubAxisSize - Elements[0].SubAxisSize;
			var spacing = !Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;

			for (int i = 1; i < Elements.Count; i++)
			{
				size -= Elements[i].SubAxisSize + spacing;
				if (size < 0f)
				{
					break;
				}

				blocks += 1;
			}

			return blocks;
		}

		void InitBlockSizes(int blocks)
		{
			BlocksSizes.Clear();

			EnsureListSize(BlocksSizes, blocks);
			EnsureListSize(Layout.StaggeredSettings.PaddingInnerStart, blocks);
			EnsureListSize(Layout.StaggeredSettings.PaddingInnerEnd, blocks);

			var padding = Layout.StaggeredSettings.PaddingInnerStart;
			for (int i = 0; i < blocks; i++)
			{
				BlocksSizes[i] = padding[i];
			}
		}

		static void EnsureListSize<T>(List<T> list, int size)
		{
			for (int i = list.Count; i < size; i++)
			{
				list.Add(default(T));
			}
		}

		int NextBlockIndex()
		{
			var index = 0;
			var min_size = BlocksSizes[0];

			for (int i = 1; i < BlocksSizes.Count; i++)
			{
				var size = BlocksSizes[i];
				if (size < min_size)
				{
					index = i;
					min_size = size;
				}
			}

			return index;
		}

		void InsertToBlock(int block_index, LayoutElementInfo element)
		{
			for (int i = ElementsGroup.Count; i <= block_index; i++)
			{
				ElementsGroup.Add(new List<LayoutElementInfo>());
			}

			var row = ElementsGroup[block_index];
			row.Add(element);

			BlocksSizes[block_index] += element.AxisSize;
			if (row.Count > 1)
			{
				var spacing = Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;
				BlocksSizes[block_index] += spacing;
			}
		}

		/// <summary>
		/// Group elements.
		/// </summary>
		protected override void Group()
		{
			ElementsGroup.Clear();

			var blocks = GetBlockSizes();
			InitBlockSizes(blocks);

			for (int i = 0; i < Elements.Count; i++)
			{
				InsertToBlock(NextBlockIndex(), Elements[i]);
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
		/// Calculate size of the group.
		/// </summary>
		/// <param name="isHorizontal">ElementsGroup are in horizontal order?</param>
		/// <param name="spacing">Spacing.</param>
		/// <param name="padding">Padding,</param>
		/// <returns>Size.</returns>
		protected override Vector2 CalculateGroupSize(bool isHorizontal, Vector2 spacing, Vector2 padding)
		{
			var size = default(Vector2);

			var padding_end = Layout.StaggeredSettings.PaddingInnerEnd;
			for (int i = 0; i < BlocksSizes.Count; i++)
			{
				size.x = Mathf.Max(size.x, BlocksSizes[i] + padding_end[i]);
			}

			size.y = GetSubAxisSize();

			return ByAxis(size);
		}

		float GetSubAxisSize()
		{
			CalculateSubAxisSizes();
			var spacing = !Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;

			return Sum(SubAxisSizes) + ((SubAxisSizes.Count - 1) * spacing);
		}

		/// <summary>
		/// Get main axis data.
		/// </summary>
		/// <param name="blockIndex">Block index.</param>
		/// <returns>Main axis data.</returns>
		protected override AxisData MainAxisData(int blockIndex)
		{
			var axis = base.MainAxisData(blockIndex);
			axis.Offset += Layout.StaggeredSettings.PaddingInnerStart[blockIndex];

			return axis;
		}
	}
}