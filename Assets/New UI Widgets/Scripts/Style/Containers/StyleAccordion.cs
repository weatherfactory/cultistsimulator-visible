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
		/// Style for the toggle background.
		/// </summary>
		[SerializeField]
		public StyleImage ToggleActiveBackground;

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
				if (item.Open)
				{
					ToggleActiveBackground.ApplyTo(item.ToggleObject.GetComponent<Image>());
				}
				else
				{
					ToggleBackground.ApplyTo(item.ToggleObject.GetComponent<Image>());
				}

				ToggleText.ApplyTo(item.ToggleObject.transform.Find("Text"));
			}

			if (item.ContentObject != null)
			{
				ContentBackground.ApplyTo(item.ContentObject.GetComponent<Image>());
				ContentText.ApplyTo(item.ContentObject.transform.Find("Text"));
			}
		}

		/// <summary>
		/// Set style options from the accordion item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void GetFrom(AccordionItem item)
		{
			if (item == null)
			{
				return;
			}

			if (item.ToggleObject != null)
			{
				if (item.Open)
				{
					ToggleActiveBackground.GetFrom(item.ToggleObject.GetComponent<Image>());
				}
				else
				{
					ToggleBackground.GetFrom(item.ToggleObject.GetComponent<Image>());
				}

				ToggleText.GetFrom(item.ToggleObject.transform.Find("Text"));
			}

			if (item.ContentObject != null)
			{
				ContentBackground.GetFrom(item.ContentObject.GetComponent<Image>());
				ContentText.GetFrom(item.ContentObject.transform.Find("Text"));
			}
		}

#if UNITY_EDITOR
		/// <inheritdoc/>
		public void SetDefaultValues()
		{
			ToggleBackground.SetDefaultValues();
			ToggleActiveBackground.SetDefaultValues();
			ToggleText.SetDefaultValues();
			ContentBackground.SetDefaultValues();
			ContentText.SetDefaultValues();
		}
#endif
	}
}