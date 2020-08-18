namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the button.
	/// </summary>
	[Serializable]
	public class StyleButton : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

		/// <summary>
		/// Style for the mask.
		/// </summary>
		[SerializeField]
		public StyleImage Mask;

		/// <summary>
		/// Style for the border.
		/// </summary>
		[SerializeField]
		public StyleImage Border;

		/// <summary>
		/// Style for the text.
		/// </summary>
		[SerializeField]
		public StyleText Text;

		/// <summary>
		/// Style for the image.
		/// </summary>
		[SerializeField]
		public StyleImage Image;

		/// <summary>
		/// Apply style to the specified gameobject.
		/// </summary>
		/// <param name="go">Gameobject.</param>
		public virtual void ApplyTo(GameObject go)
		{
			if (go != null)
			{
				var support = go.GetComponent<StyleSupportButton>();

				if (support != null)
				{
					ApplyTo(support);
				}
				else
				{
					var button = go.GetComponent<Button>();

					if (button != null)
					{
						ApplyTo(button);
					}
				}
			}
		}

		/// <summary>
		/// Apply style to the specified transform.
		/// </summary>
		/// <param name="transform">Trasnform.</param>
		public virtual void ApplyTo(Transform transform)
		{
			if (transform != null)
			{
				ApplyTo(transform.gameObject);
			}
		}

		/// <summary>
		/// Apply style for the specified button.
		/// </summary>
		/// <param name="button">Button.</param>
		public virtual void ApplyTo(StyleSupportButton button)
		{
			if (button == null)
			{
				return;
			}

			Background.ApplyTo(button.Background);
			Mask.ApplyTo(button.Mask);
			Border.ApplyTo(button.Border);
			Text.ApplyTo(button.Text);
			Image.ApplyTo(button.Image);
		}

		/// <summary>
		/// Apply style for the specified button.
		/// </summary>
		/// <param name="button">Button.</param>
		public virtual void ApplyTo(Button button)
		{
			if (button == null)
			{
				return;
			}

			if (button.transform.Find("Mask") == null)
			{
				return;
			}

			Background.ApplyTo(button.transform.Find("Mask/Texture"));
			Mask.ApplyTo(button.transform.Find("Mask"));
			Border.ApplyTo(button.transform.Find("Border"));
			Text.ApplyTo(button.transform.Find("Text"));
			Image.ApplyTo(button.transform.Find("Image"));
		}

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
			Mask.SetDefaultValues();
			Border.SetDefaultValues();
			Text.SetDefaultValues();
			Image.SetDefaultValues();
		}
#endif
	}
}