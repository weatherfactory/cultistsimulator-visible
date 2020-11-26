namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the Dropdown.
	/// </summary>
	[Serializable]
	public class StyleDropdown : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

		/// <summary>
		/// Style for the selected item text.
		/// </summary>
		[SerializeField]
		public StyleText Label;

		/// <summary>
		/// Style for the arrow.
		/// </summary>
		[SerializeField]
		public StyleImage Arrow;

		/// <summary>
		/// Style for the options background.
		/// </summary>
		[SerializeField]
		public StyleImage OptionsBackground;

		/// <summary>
		/// Style for the item background.
		/// </summary>
		[SerializeField]
		public StyleImage ItemBackground;

		/// <summary>
		/// Style for the checkmark.
		/// </summary>
		[SerializeField]
		public StyleImage ItemCheckmark;

		/// <summary>
		/// Style for the item text.
		/// </summary>
		[SerializeField]
		public StyleText ItemLabel;

#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
		/// <summary>
		/// Apply style to the specified slider.
		/// </summary>
		/// <param name="component">Slider.</param>
		/// <param name="style">Style.</param>
		public virtual void ApplyTo(Dropdown component, Style style)
		{
			if (component == null)
			{
				return;
			}

			Background.ApplyTo(component.GetComponent<Image>());
			Label.ApplyTo(component.captionText);
			Arrow.ApplyTo(component.transform.Find("Arrow"));
			OptionsBackground.ApplyTo(component.template);

			var scroll_rect = component.template.GetComponent<ScrollRect>();
			OptionsBackground.ApplyTo(scroll_rect.viewport);

			if (scroll_rect.horizontalScrollbar != null)
			{
				style.ApplyTo(scroll_rect.horizontalScrollbar.gameObject);
			}

			if (scroll_rect.verticalScrollbar != null)
			{
				style.ApplyTo(scroll_rect.verticalScrollbar.gameObject);
			}

			var item = component.itemText.transform.parent;

			if (item != null)
			{
				ItemBackground.ApplyTo(item.Find("Item Background"));

				var toggle = item.GetComponent<Toggle>();
				if (toggle != null)
				{
					ItemCheckmark.ApplyTo(toggle.graphic as Image);
				}
			}

			ItemLabel.ApplyTo(component.itemText);
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			Label.SetDefaultValues();
			Arrow.SetDefaultValues();

			OptionsBackground.SetDefaultValues();

			ItemBackground.SetDefaultValues();
			ItemCheckmark.SetDefaultValues();
			ItemLabel.SetDefaultValues();
		}
#endif
	}
}