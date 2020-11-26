#if UIWIDGETS_TMPRO_SUPPORT
namespace UIWidgets.TMProSupport
{
	using TMPro;
	using UIWidgets.Styles;
	using UnityEngine;

	/// <summary>
	/// Style support for TextMeshPro.
	/// </summary>
	public static class StyleTMPro
	{
		/// <summary>
		/// Apply style for the specified gameobject.
		/// </summary>
		/// <param name="style">Style.</param>
		/// <param name="go">Gameobject.</param>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		public static bool ApplyTo(StyleText style, GameObject go)
		{
			var applied = false;

			if (go != null)
			{
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
				applied |= ApplyTo(style, go.GetComponent<TMP_InputField>());
				applied |= ApplyTo(style, go.GetComponent<TMP_Text>());
#else
				applied |= ApplyTo(style, go.GetComponent<TextMeshProUGUI>());
#endif

			}

			return applied;
		}

#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
		/// <summary>
		/// Apply style for the specified InputField.
		/// </summary>
		/// <param name="style">Style.</param>
		/// <param name="component">InputField.</param>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		public static bool ApplyTo(StyleText style, TMP_InputField component)
		{
			if (component == null)
			{
				return false;
			}

			ApplyTo(style, component.textComponent, true);

			if (component.placeholder != null)
			{
				ApplyTo(style, component.placeholder.GetComponent<TextMeshProUGUI>(), true);
			}

			return true;
		}
#endif

		/// <summary>
		/// Apply style for the specified Text.
		/// </summary>
		/// <param name="style">Style for text.</param>
		/// <param name="component">Text.</param>
		/// <param name="isInputField">Is transform belongs to the InputField component?</param>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
		public static bool ApplyTo(StyleText style, TMP_Text component, bool isInputField = false)
#else
		public static bool ApplyTo(StyleText style, TextMeshProUGUI component, bool isInputField = false)
#endif
		{
			if (component == null)
			{
				return false;
			}

			if (style.ChangeFont && (style.FontTMPro != null))
			{
				component.font = style.FontTMPro;
			}

			if (style.ChangeFontStyle)
			{
				component.fontStyle = ConvertStyle(style.FontStyle);
			}

			if (style.ChangeLineSpacing)
			{
				component.lineSpacing = style.LineSpacing;
			}

			if (style.ChangeRichText && (!isInputField))
			{
				component.richText = style.RichText;
			}

			if (style.ChangeAlignment && (!isInputField))
			{
				component.alignment = ConvertAlignment(style.Alignment);
			}

			if (style.ChangeHorizontalOverflow)
			{
				// component.horizontalOverflow = style.HorizontalOverflow;
			}

			if (style.ChangeVerticalOverflow)
			{
				// component.verticalOverflow = style.VerticalOverflow;
			}

			if (style.ChangeBestFit && (!isInputField))
			{
				component.enableAutoSizing = style.BestFit;
				component.fontSizeMin = style.MinSize;
				component.fontSizeMax = style.MaxSize;
			}

			if (style.ChangeColor)
			{
				component.color = style.Color;
			}

			if (style.ChangeMaterial)
			{
				component.material = style.Material;
			}

			component.SetAllDirty();

			return true;
		}

		/// <summary>
		/// Convert alignment.
		/// </summary>
		/// <param name="alignment">Unity alignment.</param>
		/// <returns>TMPro alignment.</returns>
		public static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
		{
			// upper
			if (alignment == TextAnchor.UpperLeft)
			{
				return TextAlignmentOptions.TopLeft;
			}

			if (alignment == TextAnchor.UpperCenter)
			{
				return TextAlignmentOptions.Top;
			}

			if (alignment == TextAnchor.UpperRight)
			{
				return TextAlignmentOptions.TopRight;
			}

			// middle
			if (alignment == TextAnchor.MiddleLeft)
			{
				return TextAlignmentOptions.Left;
			}

			if (alignment == TextAnchor.MiddleCenter)
			{
				return TextAlignmentOptions.Center;
			}

			if (alignment == TextAnchor.MiddleRight)
			{
				return TextAlignmentOptions.Right;
			}

			// lower
			if (alignment == TextAnchor.LowerLeft)
			{
				return TextAlignmentOptions.BottomLeft;
			}

			if (alignment == TextAnchor.LowerCenter)
			{
				return TextAlignmentOptions.Bottom;
			}

			if (alignment == TextAnchor.LowerRight)
			{
				return TextAlignmentOptions.BottomRight;
			}

			return TextAlignmentOptions.TopLeft;
		}

		/// <summary>
		/// Convert font style.
		/// </summary>
		/// <param name="fontStyle">Unity font style.</param>
		/// <returns>TMPro font style.</returns>
		public static FontStyles ConvertStyle(FontStyle fontStyle)
		{
			if (fontStyle == FontStyle.Normal)
			{
				return FontStyles.Normal;
			}

			if (fontStyle == FontStyle.Bold)
			{
				return FontStyles.Bold;
			}

			if (fontStyle == FontStyle.Italic)
			{
				return FontStyles.Italic;
			}

			if (fontStyle == FontStyle.BoldAndItalic)
			{
				return FontStyles.Bold | FontStyles.Italic;
			}

			return FontStyles.Normal;
		}
	}
}
#endif