namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the InputField.
	/// </summary>
	[Serializable]
	public class StyleInputField : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

		/// <summary>
		/// Style for the text.
		/// </summary>
		[SerializeField]
		public StyleText Text;

		/// <summary>
		/// Style for the placeholder.
		/// </summary>
		[SerializeField]
		public StyleText Placeholder;

		/// <summary>
		/// Apply style to the specified InputField.
		/// </summary>
		/// <param name="component">InputField.</param>
		public virtual void ApplyTo(InputField component)
		{
			if (component == null)
			{
				return;
			}

			Background.ApplyTo(component);
			Text.ApplyTo(component.textComponent);

			if (component.placeholder != null)
			{
				Placeholder.ApplyTo(component.placeholder.gameObject);
			}
		}

		/// <summary>
		/// Apply style to the specified InputField.
		/// </summary>
		/// <param name="component">InputField.</param>
		public virtual void ApplyTo(InputFieldAdapter component)
		{
			if (component == null)
			{
				return;
			}

			Background.ApplyTo(component);

			if (component.textComponent != null)
			{
				Text.ApplyTo(component.textComponent.gameObject);
			}

			if (component.placeholder != null)
			{
				Placeholder.ApplyTo(component.placeholder.gameObject);
			}
		}

#if UIWIDGETS_TMPRO_SUPPORT && (UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER)
		/// <summary>
		/// Apply style to the specified InputField.
		/// </summary>
		/// <param name="component">Slider.</param>
		public virtual void ApplyTo(TMPro.TMP_InputField component)
		{
			if (component == null)
			{
				return;
			}

			Background.ApplyTo(component);
			Text.ApplyTo(component.textComponent.gameObject);

			if (component.placeholder != null)
			{
				Placeholder.ApplyTo(component.placeholder.gameObject);
			}
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			Text.SetDefaultValues();
			Placeholder.SetDefaultValues();
		}
#endif
	}
}