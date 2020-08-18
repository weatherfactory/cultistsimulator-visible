namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the image.
	/// </summary>
	[Serializable]
	public class StyleImage : IStyleDefaultValues
	{
		/// <summary>
		/// The sprite.
		/// </summary>
		[SerializeField]
		public Sprite Sprite;

		/// <summary>
		/// The color.
		/// </summary>
		[SerializeField]
		public Color Color = Color.white;

		/// <summary>
		/// The type.
		/// </summary>
		[SerializeField]
		public Image.Type ImageType = Image.Type.Sliced;

		/// <summary>
		/// The preserve aspect.
		/// </summary>
		[SerializeField]
		public bool PreserveAspect = false;

		/// <summary>
		/// The fill center.
		/// </summary>
		[SerializeField]
		public bool FillCenter = true;

		/// <summary>
		/// The fill method.
		/// </summary>
		[SerializeField]
		public Image.FillMethod FillMethod = Image.FillMethod.Radial360;

		/// <summary>
		/// The fill origin.
		/// </summary>
		[SerializeField]
		public int FillOrigin = 0;

		/// <summary>
		/// The fill amount.
		/// </summary>
		[SerializeField]
		[Range(0f, 1f)]
		public float FillAmount = 0;

		/// <summary>
		/// The fill clockwise.
		/// </summary>
		[SerializeField]
		public bool FillClockwise = true;

		/// <summary>
		/// The material.
		/// </summary>
		[SerializeField]
		public Material Material = null;

		/// <summary>
		/// Apply style to the specified gameobject.
		/// </summary>
		/// <param name="go">Gameobject.</param>
		public virtual void ApplyTo(GameObject go)
		{
			if (go != null)
			{
				ApplyTo(go.GetComponent<Image>());
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
		/// Apply style to the specified Image.
		/// </summary>
		/// <param name="component">Image.</param>
		public virtual void ApplyTo(Image component)
		{
			if (component == null)
			{
				return;
			}

			component.sprite = Sprite;
			component.color = Color;
			component.material = Material;

			component.type = ImageType;
			component.preserveAspect = PreserveAspect;
			component.fillCenter = FillCenter;
			component.fillMethod = FillMethod;
			component.fillOrigin = FillOrigin;
			component.fillAmount = FillAmount;
			component.fillClockwise = FillClockwise;

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