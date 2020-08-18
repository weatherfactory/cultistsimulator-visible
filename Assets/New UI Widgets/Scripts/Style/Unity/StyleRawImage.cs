namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the image.
	/// </summary>
	[Serializable]
	public class StyleRawImage : IStyleDefaultValues
	{
		/// <summary>
		/// The sprite.
		/// </summary>
		[SerializeField]
		public Texture2D Texture;

		/// <summary>
		/// The color.
		/// </summary>
		[SerializeField]
		public Color Color = Color.white;

		/// <summary>
		/// The material.
		/// </summary>
		[SerializeField]
		public Material Material = null;

		/// <summary>
		/// The UVRect.
		/// </summary>
		[SerializeField]
		public Rect UVRect = new Rect(1f, 0f, 4f, 1f);

		/// <summary>
		/// Apply style to the specified gameobject.
		/// </summary>
		/// <param name="go">Gameobject.</param>
		public virtual void ApplyTo(GameObject go)
		{
			if (go != null)
			{
				ApplyTo(go.GetComponent<RawImage>());
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
		/// Apply style to the specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		public virtual void ApplyTo(Component component)
		{
			if (component != null)
			{
				ApplyTo(component.gameObject);
			}
		}

		/// <summary>
		/// Apply style to the specified RawImage.
		/// </summary>
		/// <param name="component">RawImage.</param>
		public virtual void ApplyTo(RawImage component)
		{
			if (component == null)
			{
				return;
			}

			component.texture = Texture;
			component.color = Color;
			component.material = Material;
			component.uvRect = UVRect;

			component.SetAllDirty();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
		}
#endif
	}
}