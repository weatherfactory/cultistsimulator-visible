namespace UIWidgets
{
	using System;
	using System.Collections;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Animations.
	/// </summary>
	public static class Animations
	{
		/// <summary>
		/// Rotate animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="startAngle">Start rotation angle.</param>
		/// <param name="endAngle">End rotation angle.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator Rotate(RectTransform rectTransform, float time = 0.5f, float startAngle = 0, float endAngle = 90, bool unscaledTime = false, Action actionAfter = null)
		{
			var start_rotarion = rectTransform.localRotation.eulerAngles;

			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var rotation_x = Mathf.Lerp(startAngle, endAngle, t);

				rectTransform.localRotation = Quaternion.Euler(rotation_x, start_rotarion.y, start_rotarion.z);
				yield return null;
			}

			// return rotation back for future use
			rectTransform.localRotation = Quaternion.Euler(start_rotarion);

			if (actionAfter != null)
			{
				actionAfter();
			}
		}

		/// <summary>
		/// Rotate animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="startAngle">Start rotation angle.</param>
		/// <param name="endAngle">End rotation angle.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator RotateZ(RectTransform rectTransform, float time = 0.5f, float startAngle = 0, float endAngle = 90, bool unscaledTime = false, Action actionAfter = null)
		{
			var start_rotarion = rectTransform.localRotation.eulerAngles;
			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var rotation_z = Mathf.Lerp(startAngle, endAngle, t);

				rectTransform.localRotation = Quaternion.Euler(start_rotarion.x, start_rotarion.y, rotation_z);
				yield return null;
			}

			// return rotation back for future use
			rectTransform.localRotation = Quaternion.Euler(start_rotarion);

			if (actionAfter != null)
			{
				actionAfter();
			}
		}

		/// <summary>
		/// Collapse animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="isHorizontal">Is Horizontal animated width changes; otherwise height.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator Collapse(RectTransform rectTransform, float time = 0.5f, bool isHorizontal = false, bool unscaledTime = false, Action actionAfter = null)
		{
			var layoutElement = Utilities.GetOrAddComponent<LayoutElement>(rectTransform);
			var size = isHorizontal
				? (LayoutUtilities.IsWidthControlled(rectTransform) ? LayoutUtility.GetPreferredWidth(rectTransform) : rectTransform.rect.width)
				: (LayoutUtilities.IsHeightControlled(rectTransform) ? LayoutUtility.GetPreferredHeight(rectTransform) : rectTransform.rect.height);

			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var new_size = Mathf.Lerp(size, 0, t);
				if (isHorizontal)
				{
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new_size);
					layoutElement.preferredWidth = new_size;
				}
				else
				{
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, new_size);
					layoutElement.preferredHeight = new_size;
				}

				yield return null;
			}

			// return size back for future use
			if (isHorizontal)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				layoutElement.preferredWidth = size;
			}
			else
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				layoutElement.preferredHeight = size;
			}

			if (actionAfter != null)
			{
				actionAfter();
			}
		}

		/// <summary>
		/// Collapse animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="isHorizontal">Is Horizontal animated width changes; otherwise height.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator CollapseFlexible(RectTransform rectTransform, float time = 0.5f, bool isHorizontal = false, bool unscaledTime = false, Action actionAfter = null)
		{
			var layoutElement = Utilities.GetOrAddComponent<LayoutElement>(rectTransform);
			if (isHorizontal)
			{
				layoutElement.preferredWidth = -1f;
			}
			else
			{
				layoutElement.preferredHeight = -1f;
			}

			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var size = Mathf.Lerp(1f, 0f, t);
				if (isHorizontal)
				{
					layoutElement.flexibleWidth = size;
				}
				else
				{
					layoutElement.flexibleHeight = size;
				}

				yield return null;
			}

			if (isHorizontal)
			{
				layoutElement.flexibleWidth = 0f;
			}
			else
			{
				layoutElement.flexibleHeight = 0f;
			}

			if (actionAfter != null)
			{
				actionAfter();
			}
		}

		/// <summary>
		/// Open animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="isHorizontal">Is Horizontal animated width changes; otherwise height.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator Open(RectTransform rectTransform, float time = 0.5f, bool isHorizontal = false, bool unscaledTime = false, Action actionAfter = null)
		{
			var layoutElement = Utilities.GetOrAddComponent<LayoutElement>(rectTransform);
			var size = isHorizontal
				? (LayoutUtilities.IsWidthControlled(rectTransform) ? LayoutUtility.GetPreferredWidth(rectTransform) : rectTransform.rect.width)
				: (LayoutUtilities.IsHeightControlled(rectTransform) ? LayoutUtility.GetPreferredHeight(rectTransform) : rectTransform.rect.height);

			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var new_size = Mathf.Lerp(0, size, t);
				if (isHorizontal)
				{
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new_size);
					layoutElement.preferredWidth = new_size;
				}
				else
				{
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, new_size);
					layoutElement.preferredHeight = new_size;
				}

				yield return null;
			}

			// return size back for future use
			if (isHorizontal)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				layoutElement.preferredWidth = size;
			}
			else
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				layoutElement.preferredHeight = size;
			}

			if (actionAfter != null)
			{
				actionAfter();
			}
		}

		/// <summary>
		/// Open animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="time">Time.</param>
		/// <param name="isHorizontal">Is Horizontal animated width changes; otherwise height.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <param name="actionAfter">Action to run after animation.</param>
		public static IEnumerator OpenFlexible(RectTransform rectTransform, float time = 0.5f, bool isHorizontal = false, bool unscaledTime = false, Action actionAfter = null)
		{
			var layoutElement = Utilities.GetOrAddComponent<LayoutElement>(rectTransform);
			if (isHorizontal)
			{
				layoutElement.preferredWidth = -1f;
			}
			else
			{
				layoutElement.preferredHeight = -1f;
			}

			var end_time = UtilitiesTime.GetTime(unscaledTime) + time;

			while (UtilitiesTime.GetTime(unscaledTime) <= end_time)
			{
				var t = 1 - ((end_time - UtilitiesTime.GetTime(unscaledTime)) / time);
				var size = Mathf.Lerp(0f, 1f, t);
				if (isHorizontal)
				{
					layoutElement.flexibleWidth = size;
				}
				else
				{
					layoutElement.flexibleHeight = size;
				}

				yield return null;
			}

			// return size back for future use
			if (isHorizontal)
			{
				layoutElement.flexibleWidth = 1f;
			}
			else
			{
				layoutElement.flexibleHeight = 1f;
			}

			if (actionAfter != null)
			{
				actionAfter();
			}
		}
	}
}