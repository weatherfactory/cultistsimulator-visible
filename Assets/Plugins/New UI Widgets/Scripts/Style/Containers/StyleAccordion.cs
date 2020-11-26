namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the accordion.
	/// </summary>
	[Serializable]
	public class StyleAccordion : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the toggle background.
		/// </summary>
		[SerializeField]
		public StyleImage ToggleBackground;

		/// <summary>
		/// Style for the toggle text.
		/// </summary>
		[SerializeField]
		public StyleText ToggleText;

		/// <summary>
		/// Style for the content background.
		/// </summary>
		[SerializeField]
		public StyleImage ContentBackground;

		/// <summary>
		/// Style for the content text.
		/// </summary>
		[SerializeField]
		public StyleText ContentText;

		/// <summary>
		/// Apply style for the accordion item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void ApplyTo(AccordionItem item)
		{
			if (item == null)
			{
				return;
			}

			if (item.ToggleObject != null)
			{
				ToggleBackground.ApplyTo(item.ToggleObject.GetComponent<Image>());
				ToggleText.ApplyTo(item.ToggleObject.transform.Find("Text"));
			}

			if (item.ContentObject != null)
			{
				ContentBackground.ApplyTo(item.ContentObject.GetComponent<Image>());
				ContentText.ApplyTo(item.ContentObject.transform.Find("Text"));
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			ToggleBackground.SetDefaultValues();
			ToggleText.SetDefaultValues();
			ContentBackground.SetDefaultValues();
			ContentText.SetDefaultValues();
		}
#endif
	}
}