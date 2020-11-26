namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TextUnity.
	/// </summary>
	public class TextUnity : ITextProxy
	{
		/// <summary>
		/// Text component.
		/// </summary>
		protected Text Component;

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
				Component.fontSize = Mathf.RoundToInt(value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextUnity"/> class.
		/// </summary>
		/// <param name="component">Component.</param>
		public TextUnity(Text component)
		{
			Component = component;
		}
	}
}