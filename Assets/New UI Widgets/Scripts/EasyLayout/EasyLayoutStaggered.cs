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
			if (ElementsGroup.Count == 0)
			{
				return 0;
			}

			if (Layout.StaggeredSettings.FixedBlocksCount)
			{
				return Mathf.Max(1, Layout.StaggeredSettings.BlocksCount);
			}

			var blocks = 1;

			var size = Layout.SubAxisSize - ElementsGroup[0].SubAxisSize;
			var spacing = !Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;

			foreach (var element in ElementsGroup.Elements)
			{
				size -= element.SubAxisSize + spacing;
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
			var block = Layout.IsHorizontal
				? ElementsGroup.GetRow(block_index)
				: ElementsGroup.GetColumn(block_index);
			if (Layout.IsHorizontal)
			{
				ElementsGroup.SetPosition(element, block_index, block.Count);
			}
			else
			{
				ElementsGroup.SetPosition(element, block.Count, block_index);
			}

			BlocksSizes[block_index] += element.AxisSize;
			if (block.Count > 0)
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
			var blocks = GetBlockSizes();
			InitBlockSizes(blocks);

			foreach (var element in ElementsGroup.Elements)
			{
				InsertToBlock(NextBlockIndex(), element);
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