namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Ellipse layout group.
	/// </summary>
	public class EasyLayoutEllipse : EasyLayoutBaseType
	{
		readonly List<LayoutElementInfo> EllipseGroup = new List<LayoutElementInfo>();

		/// <summary>
		/// Initializes a new instance of the <see cref="EasyLayoutEllipse"/> class.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public EasyLayoutEllipse(EasyLayout layout)
				: base(layout)
		{
		}

		/// <summary>
		/// Get element position in the group.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>Position.</returns>
		public override EasyLayoutPosition GetElementPosition(RectTransform element)
		{
			var target_id = element.GetInstanceID();
			for (int x = 0; x < EllipseGroup.Count; x++)
			{
				if (EllipseGroup[x].Rect.GetInstanceID() == target_id)
				{
					return new EasyLayoutPosition(x, 0);
				}
			}

			return new EasyLayoutPosition(-1, -1);
		}

		/// <summary>
		/// Calculate group size.
		/// </summary>
		/// <returns>Size.</returns>
		protected override Vector2 CalculateGroupSize()
		{
			var settings = Layout.EllipseSettings;
			var size = Layout.InternalSize;

			if (!settings.WidthAuto)
			{
				size.x = settings.Width;
			}

			if (!settings.HeightAuto)
			{
				size.y = settings.Height;
			}

			var max_size = FindElementsMaxSize();
			var align = GetAlignRate(settings.Align) * 2f;

			if (settings.ElementsRotate)
			{
				var rotation_angle = Mathf.Deg2Rad * settings.ElementsRotationStart;
				var rotation_sin = Math.Sin(rotation_angle);
				var rotation_cos = Math.Cos(rotation_angle);
				var length = (max_size.x * rotation_cos) + (max_size.y * rotation_sin);
				size.x -= Mathf.Abs((float)length) * (align + 1f);
				size.y -= Mathf.Abs((float)length) * (align + 1f);
			}
			else
			{
				size.x -= max_size.x * (align + 1f);
				size.y -= max_size.y * (align + 1f);
			}

			size.x = Mathf.Max(1f, size.x);
			size.y = Mathf.Max(1f, size.y);

			return size;
		}

		Vector2 FindElementsMaxSize()
		{
			var max_size = Vector2.zero;

			foreach (var element in ElementsGroup.Elements)
			{
				max_size.x = Mathf.Max(max_size.x, element.Width);
				max_size.y = Mathf.Max(max_size.y, element.Height);
			}

			return max_size;
		}

		/// <summary>
		/// Calculate positions of the elements.
		/// </summary>
		/// <param name="size">Size.</param>
		protected override void CalculatePositions(Vector2 size)
		{
			if (EllipseGroup.Count == 0)
			{
				return;
			}

			var settings = Layout.EllipseSettings;

			var angle_auto = settings.Fill == EllipseFill.Closed
				? 360f / EllipseGroup.Count
				: settings.ArcLength / Mathf.Max(1, EllipseGroup.Count - 1);
			var angle_step = settings.AngleStepAuto ? angle_auto : settings.AngleStep;

			var angle = settings.AngleStart + settings.AngleFiller + settings.AngleScroll;

			var center = new Vector2(size.x / 2.0f, size.y / 2.0f);
			var align = GetAlignRate(settings.Align);

			var rotation_angle_rad = Mathf.Deg2Rad * settings.ElementsRotationStart;
			var rotation_sin = (float)Math.Sin(rotation_angle_rad);
			var rotation_cos = (float)Math.Cos(rotation_angle_rad);

			var pivot = new Vector2(0.5f, 0.5f);
			for (int i = 0; i < EllipseGroup.Count; i++)
			{
				var element = EllipseGroup[i];

				element.NewPivot = pivot;

				var position_angle_rad = Mathf.Deg2Rad * angle;

				var position_sin = Mathf.Sin(position_angle_rad);
				var position_cos = Mathf.Cos(position_angle_rad);

				var element_pos = new Vector2(center.x * position_cos, center.y * position_sin);

				if (settings.ElementsRotate)
				{
					var length = Mathf.Abs(element.Width * rotation_cos) + Mathf.Abs(element.Height * rotation_sin);
					var align_fix = new Vector2(
						length * position_cos * align,
						length * position_sin * align);
					element_pos += align_fix;
				}
				else
				{
					var align_fix = new Vector2(
						element.Width * position_cos * align,
						element.Height * position_sin * align);
					element_pos += align_fix;
				}

				element.NewEulerAnglesZ = settings.ElementsRotate ? (angle + settings.ElementsRotationStart) : 0f;

				element.PositionPivot = element_pos;

				angle -= angle_step;
			}
		}

		static float GetAlignRate(EllipseAlign align)
		{
			switch (align)
			{
				case EllipseAlign.Outer:
					return -0.5f;
				case EllipseAlign.Center:
					return 0f;
				case EllipseAlign.Inner:
					return 0.5f;
				default:
					Debug.LogWarning("Unknown ellipse align: " + align);
					break;
			}

			return 0f;
		}

		/// <summary>
		/// Calculate sizes of the elements.
		/// </summary>
		protected override void CalculateSizes()
		{
		}

		/// <summary>
		/// Group elements.
		/// </summary>
		protected override void Group()
		{
			EllipseGroup.Clear();
			EllipseGroup.AddRange(ElementsGroup.Elements);

			if (Layout.RightToLeft)
			{
				EllipseGroup.Reverse();
			}
		}
	}
}