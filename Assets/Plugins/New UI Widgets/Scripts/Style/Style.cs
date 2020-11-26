namespace UIWidgets.Styles
{
	using System;
	using System.Collections.Generic;
#if UNITY_EDITOR
	using UnityEditor;
#endif
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// Style.
	/// </summary>
	public class Style : ScriptableObject, IStyleDefaultValues
	{
		/// <summary>
		/// Is default style?
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with PrefabsMenu.Instance.DefaultStyle.")]
		protected bool Default;

		/// <summary>
		/// Style for the collections.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("Common")]
		[FormerlySerializedAs("Short")]
		public StyleFast Fast;

		/// <summary>
		/// Style for the collections.
		/// </summary>
		[Header("Collections")]
		[Tooltip("ListView's, TiveView's and TreeView's style")]
		[SerializeField]
		public StyleCollections Collections;

		/// <summary>
		/// Style for the TreeView.
		/// </summary>
		[SerializeField]
		public StyleTreeView TreeView;

		/// <summary>
		/// Style for the combobox.
		/// </summary>
		[SerializeField]
		public StyleCombobox Combobox;

		/// <summary>
		/// Style for the Table.
		/// </summary>
		[SerializeField]
		public StyleTable Table;

		/// <summary>
		/// Style for the FileListView.
		/// </summary>
		[SerializeField]
		public StyleFileListView FileListView;

		/// <summary>
		/// Style for the IO errors for collections.
		/// </summary>
		[SerializeField]
		public StyleText IOCollectionsErrors;

		/// <summary>
		/// Style for the accordion.
		/// </summary>
		[Header("Containers")]
		[SerializeField]
		public StyleAccordion Accordion;

		/// <summary>
		/// Style for the tabs on top.
		/// </summary>
		[SerializeField]
		public StyleTabs TabsTop;

		/// <summary>
		/// Style for the tabs on left.
		/// </summary>
		[SerializeField]
		public StyleTabs TabsLeft;

		/// <summary>
		/// Style for the Dialog.
		/// </summary>
		[Header("Dialogs")]
		[SerializeField]
		public StyleDialog Dialog;

		/// <summary>
		/// Style for the Notify.
		/// </summary>
		[SerializeField]
		public StyleNotify Notify;

		/// <summary>
		/// Style for the Autocomplete.
		/// </summary>
		[Header("Input")]
		[SerializeField]
		public StyleAutocomplete Autocomplete;

		/// <summary>
		/// Style for the big button.
		/// </summary>
		[SerializeField]
		public StyleButton ButtonBig;

		/// <summary>
		/// Style for the small button.
		/// </summary>
		[SerializeField]
		public StyleButton ButtonSmall;

		/// <summary>
		/// Style for the calendar.
		/// </summary>
		[SerializeField]
		public StyleCalendar Calendar;

		/// <summary>
		/// Style for the ScrollBlock.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("Scroller")]
		public StyleScroller ScrollBlock;

		/// <summary>
		/// Style for the horizontal centered slider.
		/// </summary>
		[SerializeField]
		public StyleCenteredSlider CenteredSliderHorizontal;

		/// <summary>
		/// Style for the vertical centered slider.
		/// </summary>
		[SerializeField]
		public StyleCenteredSlider CenteredSliderVertical;

		/// <summary>
		/// Style for the ColorPicker.
		/// </summary>
		[SerializeField]
		public StyleColorPicker ColorPicker;

		/// <summary>
		/// Style for the horizontal color picker range.
		/// </summary>
		[SerializeField]
		public StyleColorPickerRange ColorPickerRangeHorizontal;

		/// <summary>
		/// Style for the vertical color picker range.
		/// </summary>
		[SerializeField]
		public StyleColorPickerRange ColorPickerRangeVertical;

		/// <summary>
		/// Style for the horizontal range slider.
		/// </summary>
		[SerializeField]
		public StyleRangeSlider RangeSliderHorizontal;

		/// <summary>
		/// Style for the vertical range slider.
		/// </summary>
		[SerializeField]
		public StyleRangeSlider RangeSliderVertical;

		/// <summary>
		/// Style for the spinner.
		/// </summary>
		[SerializeField]
		public StyleSpinner Spinner;

		/// <summary>
		/// Style for the switch.
		/// </summary>
		[SerializeField]
		public StyleSwitch Switch;

		/// <summary>
		/// Style for the time widget.
		/// </summary>
		[SerializeField]
		public StyleTime Time;

		/// <summary>
		/// Style for the audio player.
		/// </summary>
		[Header("Misc")]
		[SerializeField]
		public StyleAudioPlayer AudioPlayer;

		/// <summary>
		/// Style for the determinate progress bar.
		/// </summary>
		[SerializeField]
		public StyleProgressbarDeterminate ProgressbarDeterminate;

		/// <summary>
		/// Style for the indeterminate progress bar.
		/// </summary>
		[SerializeField]
		public StyleProgressbarIndeterminate ProgressbarIndeterminate;

		/// <summary>
		/// Style for the paginator.
		/// </summary>
		[SerializeField]
		public StyleTooltip Tooltip;

		/// <summary>
		/// Style for the paginator.
		/// </summary>
		[SerializeField]
		public StylePaginator Paginator;

		/// <summary>
		/// Style for the close button.
		/// </summary>
		[SerializeField]
		public StyleButtonClose ButtonClose;

		/// <summary>
		/// Style for the Sidebar.
		/// </summary>
		[SerializeField]
		public StyleSidebar Sidebar;

		/// <summary>
		/// Style for the canvas.
		/// </summary>
		[Header("Default Unity Widgets")]
		[SerializeField]
		public StyleCanvas Canvas;

		/// <summary>
		/// Style for the text.
		/// </summary>
		[SerializeField]
		public StyleText Text;

		/// <summary>
		/// Style for the horizontal scrollbar.
		/// </summary>
		[SerializeField]
		public StyleScrollbar HorizontalScrollbar;

		/// <summary>
		/// Style for the vertical scrollbar.
		/// </summary>
		[SerializeField]
		public StyleScrollbar VerticalScrollbar;

		/// <summary>
		/// Style for the input field.
		/// </summary>
		[SerializeField]
		public StyleInputField InputField;

		/// <summary>
		/// Style for the button.
		/// </summary>
		[SerializeField]
		public StyleUnityButton Button;

		/// <summary>
		/// Style for the slider.
		/// </summary>
		[SerializeField]
		public StyleSlider Slider;

		/// <summary>
		/// Style for the toggle.
		/// </summary>
		[SerializeField]
		public StyleToggle Toggle;

		/// <summary>
		/// Style for the Dropdown.
		/// </summary>
		[SerializeField]
		public StyleDropdown Dropdown;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Reviewed")]
		static Style()
		{
			#if UIWIDGETS_TMPRO_SUPPORT
			TMProSupport = UIWidgets.TMProSupport.StyleTMPro.ApplyTo;
			#else
			TMProSupport = NoTMProSupport;
			#endif
		}

		static bool NoTMProSupport(StyleText style, GameObject go)
		{
			return false;
		}

		/// <summary>
		/// The function to process TMPro gameobject.
		/// </summary>
		public static Func<StyleText, GameObject, bool> TMProSupport;

		/// <summary>
		/// Apply style for the specified component.
		/// </summary>
		/// <returns><c>true</c>, if style was applied for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Component component)
		{
			var applied_for_children = false;
			Text.ApplyTo(component as Text);
			applied_for_children |= ApplyTo(component as Scrollbar);
			applied_for_children |= ApplyTo(component as InputField);
			applied_for_children |= ApplyTo(component as Button);
			applied_for_children |= ApplyTo(component as Slider);
			applied_for_children |= ApplyTo(component as Toggle);
			ApplyTo(component as Canvas);
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
			applied_for_children |= ApplyTo(component as Dropdown);
#endif

			return applied_for_children;
		}

#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
		/// <summary>
		/// Apply style for the Dropdown.
		/// </summary>
		/// <returns><c>true</c>, if style was applyed for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Dropdown component)
		{
			if (component == null)
			{
				return false;
			}

			Dropdown.ApplyTo(component, this);

			return true;
		}
#endif

		/// <summary>
		/// Apply style for the InputField.
		/// </summary>
		/// <returns><c>true</c>, if style was applied for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Toggle component)
		{
			if (component == null)
			{
				return false;
			}

			Toggle.ApplyTo(component);

			return true;
		}

		/// <summary>
		/// Apply style for the Canvas.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void ApplyTo(Canvas component)
		{
			if (component != null)
			{
				Canvas.ApplyTo(component);
			}
		}

		/// <summary>
		/// Apply style for the Slider.
		/// </summary>
		/// <returns><c>true</c>, if style was applyed for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Slider component)
		{
			if (component == null)
			{
				return false;
			}

			Slider.ApplyTo(component);

			return true;
		}

		/// <summary>
		/// Apply style for the InputField.
		/// </summary>
		/// <returns><c>true</c>, if style was applied for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(InputField component)
		{
			if (component == null)
			{
				return false;
			}

			InputField.ApplyTo(component);

			return true;
		}

		/// <summary>
		/// Apply style for the button.
		/// </summary>
		/// <returns><c>true</c>, if style was applied for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Button component)
		{
			if (component == null)
			{
				return false;
			}

			Button.ApplyTo(component);

			return true;
		}

		/// <summary>
		/// Apply style for the scrollbar.
		/// </summary>
		/// <returns><c>true</c>, if style was applied for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="component">Component.</param>
		protected virtual bool ApplyTo(Scrollbar component)
		{
			if (component == null)
			{
				return false;
			}

			if (component.direction == Scrollbar.Direction.LeftToRight || component.direction == Scrollbar.Direction.RightToLeft)
			{
				HorizontalScrollbar.ApplyTo(component);
			}
			else
			{
				VerticalScrollbar.ApplyTo(component);
			}

			return true;
		}

		/// <summary>
		/// Apply style for the gameobject with specified transform.
		/// </summary>
		/// <param name="target">Transform.</param>
		/// <param name="stylableOnly">Is style should be applied only for objects with IStylable component?</param>
		public virtual void ApplyTo(Transform target, bool stylableOnly = false)
		{
			if (target == null)
			{
				return;
			}

			ApplyTo(target.gameObject, stylableOnly);
		}

		/// <summary>
		/// Apply style for the specified gameobject.
		/// </summary>
		/// <param name="target">Gameobject.</param>
		/// <param name="stylableOnly">Is style should be applied only for objects with IStylable component?</param>
		public virtual void ApplyTo(GameObject target, bool stylableOnly = false)
		{
			var stylable = new List<IStylable>();
			Compatibility.GetComponents(target, stylable);

			var applied_for_children = false;

			if (!stylableOnly && (stylable.Count == 0))
			{
				var components = new List<Component>();
				Compatibility.GetComponents(target, components);
				foreach (var component in components)
				{
					applied_for_children |= ApplyTo(component);
				}

				if (!applied_for_children)
				{
					applied_for_children |= TMProSupport(Text, target);
				}
			}

			foreach (var component in stylable)
			{
				applied_for_children |= component.SetStyle(this);
			}

			if (!applied_for_children)
			{
				foreach (Transform child in target.transform)
				{
					ApplyTo(child.gameObject, stylableOnly);
				}
			}
		}

		/// <summary>
		/// Apply style for children gameobject.
		/// </summary>
		/// <param name="parent">Parent.</param>
		/// <param name="stylableOnly">Is style should be applied only for objects with IStylable component?</param>
		public virtual void ApplyForChildren(GameObject parent, bool stylableOnly = false)
		{
			foreach (Transform child in parent.transform)
			{
				ApplyTo(child.gameObject, stylableOnly);
			}
		}

		/// <summary>
		/// Apply style to specified target.
		/// </summary>
		/// <param name="tagret">Tagret.</param>
		/// <param name="style">Style.</param>
		public static void ApplyTo(GameObject tagret, Style style)
		{
			style.ApplyTo(tagret);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed.")]
		public virtual void SetDefaultValues()
		{
			Fast.SetDefaultValues();

			Collections.SetDefaultValues();
			TreeView.SetDefaultValues();
			Combobox.SetDefaultValues();
			Table.SetDefaultValues();
			FileListView.SetDefaultValues();
			IOCollectionsErrors.SetDefaultValues();

			Accordion.SetDefaultValues();
			TabsTop.SetDefaultValues();
			TabsLeft.SetDefaultValues();

			Dialog.SetDefaultValues();
			Notify.SetDefaultValues();

			Autocomplete.SetDefaultValues();
			ButtonBig.SetDefaultValues();
			ButtonSmall.SetDefaultValues();
			Calendar.SetDefaultValues();
			CenteredSliderHorizontal.SetDefaultValues();
			CenteredSliderVertical.SetDefaultValues();
			ColorPicker.SetDefaultValues();
			ColorPickerRangeHorizontal.SetDefaultValues();
			ColorPickerRangeVertical.SetDefaultValues();
			RangeSliderHorizontal.SetDefaultValues();
			RangeSliderVertical.SetDefaultValues();
			Spinner.SetDefaultValues();
			Switch.SetDefaultValues();
			Time.SetDefaultValues();
			AudioPlayer.SetDefaultValues();

			ProgressbarDeterminate.SetDefaultValues();
			ProgressbarIndeterminate.SetDefaultValues();
			Tooltip.SetDefaultValues();

			Paginator.SetDefaultValues();
			ButtonClose.SetDefaultValues();

			Canvas.SetDefaultValues();
			Text.SetDefaultValues();
			HorizontalScrollbar.SetDefaultValues();
			VerticalScrollbar.SetDefaultValues();
			InputField.SetDefaultValues();
			Button.SetDefaultValues();
			Slider.SetDefaultValues();
			Toggle.SetDefaultValues();
			Dropdown.SetDefaultValues();
		}

		/// <summary>
		/// Is it default style? (Editor only)
		/// </summary>
		/// <returns><c>true</c> if this style is default; otherwise, <c>false</c>.</returns>
		public bool IsDefault()
		{
			return PrefabsMenu.Instance.DefaultStyle == this;
		}

		/// <summary>
		/// Undo this style as default. (Editor only)
		/// </summary>
		public void SetAsNotDefault()
		{
			Undo.RecordObject(this, "Undo default");
			Undo.RecordObject(PrefabsMenu.Instance, "Undo default");

			PrefabsMenu.Instance.DefaultStyle = null;
			EditorUtility.SetDirty(PrefabsMenu.Instance.DefaultStyle);
		}

		/// <summary>
		/// Set this style as default. (Editor only)
		/// </summary>
		public void SetAsDefault()
		{
			Undo.RecordObject(PrefabsMenu.Instance, "Undo default");

			PrefabsMenu.Instance.DefaultStyle = this;
			EditorUtility.SetDirty(PrefabsMenu.Instance.DefaultStyle);
		}

		/// <summary>
		/// Get the default style (Editor only).
		/// </summary>
		/// <returns>The default style.</returns>
		[Obsolete("Replaced with PrefabsMenu.Instance.DefaultStyle.")]
		public static Style DefaultStyle()
		{
			if (PrefabsMenu.Instance.DefaultStyle != null)
			{
				return PrefabsMenu.Instance.DefaultStyle;
			}

			foreach (var style in GetStyles())
			{
#pragma warning disable 0618
				if (style.Default)
#pragma warning restore 0618
				{
					PrefabsMenu.Instance.DefaultStyle = style;
					EditorUtility.SetDirty(PrefabsMenu.Instance);

					return style;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the list of all styles (Editor only).
		/// </summary>
		/// <returns>The styles.</returns>
		public static List<Style> GetStyles()
		{
			return Utilities.GetAssets<Style>("t:" + typeof(Style).FullName);
		}
		#endif
	}
}