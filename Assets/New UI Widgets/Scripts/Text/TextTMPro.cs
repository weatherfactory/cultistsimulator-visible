#if UIWIDGETS_TMPRO_SUPPORT
namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TextTMPro.
	/// </summary>
	public class TextTMPro : ITextProxy
	{
		/// <summary>
		/// Text component.
		/// </summary>
		protected TMPro.TextMeshProUGUI Component;

		/// <summary>
		/// GameObject.
		/// </summary>
		public GameObject GameObject
		{
			get
			{
				return Component.gameObject;
			}
		}

		/// <summary>
		/// Graphic component.
		/// </summary>
		public Graphic Graphic
		{
			get
			{
				return Component;
			}
		}

		/// <summary>
		/// Color.
		/// </summary>
		public Color color
		{
			get
			{
				return Component.color;
			}

			set
			{
				Component.color = value;
			}
		}

		/// <summary>
		/// Font size.
		/// </summary>
		public float fontSize
		{
			get
			{
				return Component.fontSize;
			}

			set
			{
				Component.fontSize = value;
			}
		}

		/// <summary>
		/// Text.
		/// </summary>
		public string text
		{
			get
			{
				return Component.text;
			}

			set
			{
				Component.text = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextTMPro"/> class.
		/// </summary>
		/// <param name="component">Component.</param>
		public TextTMPro(TMPro.TextMeshProUGUI component)
		{
			Component = component;
		}
	}
}
#endif