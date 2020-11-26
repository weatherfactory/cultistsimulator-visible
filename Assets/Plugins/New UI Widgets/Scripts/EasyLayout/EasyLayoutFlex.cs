namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Flexbox-like layout group.
	/// </summary>
	public class EasyLayoutFlex : EasyLayoutFlexOrStaggered
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutFlex"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public EasyLayoutFlex(EasyLayout layout)
				: base(layout)
		{
		}

		/// <summary>
		/// Group elements.
		/// </summary>
		protected override void Group()
		{
			ElementsGroup.Clear();

			var row = new List<LayoutElementInfo>();
			ElementsGroup.Add(row);

			if (!Layout.FlexSettings.Wrap)
			{
				row.AddRange(Elements);
				return;
			}

			var base_size = Layout.MainAxisSize;
			var size = base_size;
			var spacing = Layout.IsHorizontal ? Layout.Spacing.x : Layout.Spacing.y;

			for (int i = 0; i < Elements.Count; i++)
			{
				if (row.Count == 0)
				{
					size -= Elements[i].AxisSize;
					row.Add(Elements[i]);
					continue;
				}

				var element_size = Elements[i].AxisSize + spacing;
				if (size >= element_size)
				{
					size -= element_size;
					row.Add(Elements[i]);
				}
				else
				{
					size = base_size - Elements[i].AxisSize;

					row = new List<LayoutElementInfo>();
					ElementsGroup.Add(row);

					row.Add(Elements[i]);
				}
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
			var size = base.CalculateGroupSize(isHorizontal, spacing, padding);

			// add is_horizontal check
			var stretch_main_axis = (Layout.FlexSettings.JustifyContent == EasyLayoutFlexSettings.Content.SpaceAround)
				|| (Layout.FlexSettings.JustifyContent == EasyLayoutFlexSettings.Content.SpaceBetween)
				|| (Layout.FlexSettings.JustifyContent == EasyLayoutFlexSettings.Content.SpaceEvenly);
			if (stretch_main_axis)
			{
				size.x = ByAxis(Layout.InternalSize).x;
			}

			var stretch_sub_axis = (Layout.FlexSettings.AlignContent == EasyLayoutFlexSettings.Content.SpaceAround)
				|| (Layout.FlexSettings.AlignContent == EasyLayoutFlexSettings.Content.SpaceBetween)
				|| (Layout.FlexSettings.AlignContent == EasyLayoutFlexSettings.Content.SpaceEvenly);
			if (stretch_sub_axis)
			{
				size.y = ByAxis(Layout.InternalSize).y;
			}

			return ByAxis(size);
		}

		/// <summary>
		/// Get axis data.
		/// </summary>
		/// <param name="isMainAxis">Is main axis?</param>
		/// <param name="elements">Elements count.</param>
		/// <param name="size">Total size of the elements.</param>
		/// <returns>Axis data.</returns>
		protected override AxisData GetAxisData(bool isMainAxis, int elements, float size)
		{
			var axis = base.GetAxisData(isMainAxis, elements, size);

			var outer_size = isMainAxis ? ByAxis(Layout.InternalSize).x : ByAxis(Layout.InternalSize).y;
			var align = isMainAxis ? Layout.FlexSettings.JustifyContent : Layout.FlexSettings.AlignContent;

			if (align == EasyLayoutFlexSettings.Content.End)
			{
				axis.Offset += outer_size - (size + ((elements - 1) * axis.Spacing));
			}
			else if (align == EasyLayoutFlexSettings.Content.Center)
			{
				axis.Offset += (outer_size - (size + ((elements - 1) * axis.Spacing))) / 2f;
			}
			else if (align == EasyLayoutFlexSettings.Content.SpaceAround)
			{
				var space = (outer_size - size) / (elements * 2);
				axis.Offset += space;
				axis.Spacing = space * 2;
			}
			else if (align == EasyLayoutFlexSettings.Content.SpaceBetween)
			{
				if (elements > 1)
				{
					axis.Spacing = (outer_size - size) / (elements - 1);
				}
			}
			else if (align == EasyLayoutFlexSettings.Content.SpaceEvenly)
			{
				var space = (outer_size - size) / (elements + 1);
				axis.Offset += space;
				axis.Spacing = space;
			}

			return axis;
		}

		/// <summary>
		/// Get align rate for the items.
		/// </summary>
		/// <returns>Align rate.</returns>
		protected override float GetItemsAlignRate()
		{
			switch (Layout.FlexSettings.AlignItems)
			{
				case EasyLayoutFlexSettings.Items.Start:
					return 0f;
				case EasyLayoutFlexSettings.Items.Center:
					return 0.5f;
				case EasyLayoutFlexSettings.Items.End:
					return 1f;
				default:
					Debug.LogWarning("Unknown items align: " + Layout.FlexSettings.AlignItems);
					break;
			}

			return 0f;
		}
	}
}