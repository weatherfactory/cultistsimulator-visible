namespace UIWidgets
{
	using System;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// Color picker Image.
	/// You can use it to get colors from custom Texture2D.
	/// The texture must have the Read/Write Enabled flag set in the import settings, otherwise this function will fail.
	/// </summary>
	public class ColorPickerImagePalette : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The texture must have the Read/Write Enabled flag set in the import settings, otherwise this function will fail.")]
		Image image;

		RectTransform imageRect;

		DragListener dragListener;

		ClickListener clickListener;

		/// <summary>
		/// Gets or sets the palette.
		/// </summary>
		/// <value>The palette.</value>
		public Image Image
		{
			get
			{
				return image;
			}

			set
			{
				SetImage(value);
			}
		}

		[SerializeField]
		RectTransform imageCursor;

		/// <summary>
		/// Gets or sets the palette cursor.
		/// </summary>
		/// <value>The palette cursor.</value>
		public RectTransform ImageCursor
		{
			get
			{
				return imageCursor;
			}

			set
			{
				imageCursor = value;
				if (imageCursor != null)
				{
					UpdateView();
				}
			}
		}

		ColorPickerInputMode inputMode;

		/// <summary>
		/// Gets or sets the input mode.
		/// </summary>
		/// <value>The input mode.</value>
		public ColorPickerInputMode InputMode
		{
			get
			{
				return inputMode;
			}

			set
			{
				inputMode = value;
			}
		}

		ColorPickerPaletteMode paletteMode;

		/// <summary>
		/// Gets or sets the palette mode.
		/// </summary>
		/// <value>The palette mode.</value>
		public ColorPickerPaletteMode PaletteMode
		{
			get
			{
				return paletteMode;
			}

			set
			{
				SetPaletteMode(value);
			}
		}

		/// <summary>
		/// OnChangeRGB event.
		/// </summary>
		public ColorRGBChangedEvent OnChangeRGB = new ColorRGBChangedEvent();

		/// <summary>
		/// OnChangeHSV event.
		/// </summary>
		public ColorHSVChangedEvent OnChangeHSV = new ColorHSVChangedEvent();

		/// <summary>
		/// OnChangeAlpha event.
		/// </summary>
		public ColorAlphaChangedEvent OnChangeAlpha = new ColorAlphaChangedEvent();

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

			Image = image;
		}

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected virtual void OnEnable()
		{
			UpdateMaterial();
		}

		/// <summary>
		/// Sets the palette.
		/// </summary>
		/// <param name="value">Value.</param>
		protected virtual void SetImage(Image value)
		{
			if (dragListener != null)
			{
				dragListener.OnDragEvent.RemoveListener(OnDrag);
			}

			image = value;
			if (image != null)
			{
				imageRect = image.transform as RectTransform;

				dragListener = Utilities.GetOrAddComponent<DragListener>(image);
				dragListener.OnDragEvent.AddListener(OnDrag);

				clickListener = Utilities.GetOrAddComponent<ClickListener>(image);
				clickListener.ClickEvent.AddListener(OnDrag);
			}
			else
			{
				imageRect = null;
			}
		}

		/// <summary>
		/// Sets the palette mode.
		/// </summary>
		/// <param name="value">Value.</param>
		protected virtual void SetPaletteMode(ColorPickerPaletteMode value)
		{
			paletteMode = value;
			var is_active = paletteMode == ColorPickerPaletteMode.Image;
			gameObject.SetActive(is_active);
			if (is_active)
			{
				UpdateView();
			}
		}

		/// <summary>
		/// If in update mode?
		/// </summary>
		protected bool inUpdateMode;

		/// <summary>
		/// Values the changed.
		/// </summary>
		protected virtual void ValueChanged()
		{
			if (inUpdateMode)
			{
				return;
			}

			currentColor = GetColor();

			OnChangeRGB.Invoke(currentColor);
		}

		/// <summary>
		/// Current color.
		/// </summary>
		protected Color32 currentColor;

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(Color32 color)
		{
			currentColor = color;
			UpdateView();
		}

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(ColorHSV color)
		{
			currentColor = color;
			UpdateView();
		}

		/// <summary>
		/// When draging is occuring this will be called every time the cursor is moved.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		protected virtual void OnDrag(PointerEventData eventData)
		{
			if (!imageCursor.gameObject.activeSelf)
			{
				imageCursor.gameObject.SetActive(true);
			}

			Vector2 size = imageRect.rect.size;
			Vector2 cur_pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, eventData.position, eventData.pressEventCamera, out cur_pos);

			cur_pos.x = Mathf.Clamp(cur_pos.x, 0, size.x);
			cur_pos.y = Mathf.Clamp(cur_pos.y, -size.y, 0);
			imageCursor.localPosition = cur_pos;

			ValueChanged();
		}

		/// <summary>
		/// Gets the color.
		/// </summary>
		/// <returns>The color.</returns>
		protected Color32 GetColor()
		{
			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Image:
					return GetPixelColorUnderCursor();
				default:
					return currentColor;
			}
		}

		/// <summary>
		/// Gets the pixel color under cursor.
		/// </summary>
		/// <returns>The pixel color under cursor.</returns>
		protected virtual Color GetPixelColorUnderCursor()
		{
			var cursor = imageCursor.localPosition;
			var size = imageRect.rect.size;

			var texture = Image.sprite.texture;
			var x = Mathf.RoundToInt(cursor.x / size.x * texture.width);
			var y = texture.height - Mathf.RoundToInt(-cursor.y / size.y * texture.height);

			return texture.GetPixel(x, y);
		}

		/// <summary>
		/// Updates the view.
		/// </summary>
		protected virtual void UpdateView()
		{
			UpdateViewReal();
		}

		/// <summary>
		/// Difference between colors.
		/// </summary>
		/// <returns>The delta, 0 if colors equal.</returns>
		/// <param name="x">The first color.</param>
		/// <param name="y">The second color.</param>
		protected virtual float ColorDelta(Color x, Color y)
		{
			return Mathf.Abs(x.r - y.r) + Mathf.Abs(x.g - y.g) + Mathf.Abs(x.b - y.b);
		}

		/// <summary>
		/// Cache texture data.
		/// </summary>
		public bool CacheTextureData = true;

		/// <summary>
		/// The cached texture identifier.
		/// </summary>
		protected int CachedTextureId;

		/// <summary>
		/// Cached texture pixels.
		/// </summary>
		protected Color[] CachedTexturePixels = Compatibility.EmptyArray<Color>();

		/// <summary>
		/// Get color coords with specified texture.
		/// </summary>
		/// <returns>The texture coords, [-1, -1] if not found.</returns>
		/// <param name="color">Color.</param>
		/// <param name="texture">Texture.</param>
		/// <param name="diff">Allowable difference between colors.</param>
		protected int[] Color2TextureCoords(Color color, Texture2D texture, float diff = 0f)
		{
			if (CacheTextureData)
			{
				if ((CachedTextureId != texture.GetInstanceID()) || (CachedTexturePixels.Length == 0))
				{
					CachedTexturePixels = texture.GetPixels();
				}

				var pos = Array.IndexOf<Color>(CachedTexturePixels, color);

				if (pos != -1)
				{
					int texture_y = pos / texture.width;
					int texture_x = pos - (texture_y * texture.width);

					return new int[] { texture_x, texture_y };
				}
				else
				{
					return new int[] { -1, -1 };
				}
			}
			else
			{
				var pixels = texture.GetPixels();
				int pos = 0;
				float delta = ColorDelta(color, pixels[0]);

				for (int i = 1; i < pixels.Length; i++)
				{
					var new_delta = diff > 0
						? ColorDelta(color, pixels[i])
						: (color == pixels[i] ? 0 : 1);
					if (new_delta < delta)
					{
						delta = new_delta;
						pos = i;
						if (delta == 0)
						{
							break;
						}
					}
				}

				if (delta <= diff)
				{
					int texture_y = pos / texture.width;
					int texture_x = pos - (texture_y * texture.width);

					return new int[] { texture_x, texture_y };
				}
				else
				{
					return new int[] { -1, -1 };
				}
			}
		}

		/// <summary>
		/// Updates the view real.
		/// </summary>
		protected virtual void UpdateViewReal()
		{
			if (Image.sprite == null)
			{
				return;
			}

			inUpdateMode = true;

			// set image drag position
			if ((imageCursor != null) && (image != null) && (imageRect != null) && (GetPixelColorUnderCursor() != currentColor))
			{
				var texture = Image.sprite.texture;

				var coords = Color2TextureCoords(currentColor, texture);

				// if coords not found hide cursor
				if ((coords[0] == -1) && (coords[1] == -1))
				{
					imageCursor.gameObject.SetActive(false);
				}
				else
				{
					var size = imageRect.rect.size;
					var x = (float)coords[0] / (float)texture.width;
					var y = (float)coords[1] / (float)texture.height;

					imageCursor.gameObject.SetActive(true);
					imageCursor.localPosition = new Vector3(x * size.x, (y - 1) * size.y, 0);
				}
			}

			inUpdateMode = false;
		}

		/// <summary>
		/// Updates the material.
		/// </summary>
		protected virtual void UpdateMaterial()
		{
			UpdateViewReal();
		}

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected virtual void OnDestroy()
		{
			dragListener = null;
		}

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <param name="styleColorPicker">Style for the ColorPicker.</param>
		/// <param name="style">Style data.</param>
		public virtual void SetStyle(StyleColorPicker styleColorPicker, Style style)
		{
			styleColorPicker.PaletteBorder.ApplyTo(image.transform.parent.GetComponent<Image>());

			if (imageCursor != null)
			{
				styleColorPicker.PaletteCursor.ApplyTo(imageCursor.GetComponent<Image>());
			}
		}

		/// <summary>
		/// Set style options from widget properties.
		/// </summary>
		/// <param name="styleColorPicker">Style for the ColorPicker.</param>
		/// <param name="style">Style data.</param>
		public virtual void GetStyle(StyleColorPicker styleColorPicker, Style style)
		{
			styleColorPicker.PaletteBorder.GetFrom(image.transform.parent.GetComponent<Image>());

			if (imageCursor != null)
			{
				styleColorPicker.PaletteCursor.GetFrom(imageCursor.GetComponent<Image>());
			}
		}
	}
}