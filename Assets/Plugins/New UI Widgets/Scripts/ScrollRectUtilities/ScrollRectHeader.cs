namespace UIWidgets
{
	using System;
	using UIWidgets.Attributes;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ScrollRectHeader.
	/// </summary>
	public class ScrollRectHeader : MonoBehaviourConditional
	{
		/// <summary>
		/// ScrollRect.
		/// </summary>
		[SerializeField]
		protected ScrollRect ScrollRect;

		/// <summary>
		/// Header.
		/// </summary>
		[SerializeField]
		protected RectTransform Header;

		/// <summary>
		/// Is ScrollRect has horizontal direction.
		/// </summary>
		[SerializeField]
		public bool IsHorizontal = false;

		/// <summary>
		/// Header type.
		/// </summary>
		[SerializeField]
		protected ScrollRectHeaderType HeaderType = ScrollRectHeaderType.Reveal;

		/// <summary>
		/// Min size of the header.
		/// </summary>
		[SerializeField]
		[EditorConditionEnum("HeaderType", new int[] { (int)ScrollRectHeaderType.Resize })]
		public float HeaderMinSize = 30f;

		/// <summary>
		/// Last position of the ScrollRect.content.
		/// </summary>
		[NonSerialized]
		protected Vector2 LastPosition;

		/// <summary>
		/// Header size.
		/// </summary>
		[NonSerialized]
		protected Vector2 HeaderSize;

		[NonSerialized]
		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
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

			if ((ScrollRect != null) && (Header != null))
			{
				ScrollRect.onValueChanged.AddListener(Scroll);
				LastPosition = ScrollRect.content.anchoredPosition;
				HeaderSize = Header.rect.size;
			}
		}

		/// <summary>
		/// Process ScrollRect.onValueChanged event.
		/// </summary>
		/// <param name="scrollPosition">ScrollRect value.</param>
		protected void Scroll(Vector2 scrollPosition)
		{
			if ((scrollPosition.x > 1f) || (scrollPosition.x < 0f) || (scrollPosition.y > 1f) || (scrollPosition.y < 0f))
			{
				return;
			}

			var position = ScrollRect.content.anchoredPosition;
			switch (HeaderType)
			{
				case ScrollRectHeaderType.Resize:
					Resize(position);
					break;
				case ScrollRectHeaderType.Reveal:
					Reveal(position);
					break;
				default:
					throw new NotSupportedException("Unknown ScrollRectHeaderType: " + HeaderType);
			}

			LastPosition = position;
		}

		/// <summary>
		/// Reveal header.
		/// </summary>
		/// <param name="position">Current ScrollRect.content position.</param>
		protected void Reveal(Vector2 position)
		{
			var header_pos = Utilities.GetTopLeftCorner(Header);
			var diff = IsHorizontal
				? -position.x - (-LastPosition.x)
				: position.y - LastPosition.y;
			if (IsHorizontal)
			{
				header_pos.x = Mathf.Clamp(header_pos.x - diff, -HeaderSize.x, 0f);
			}
			else
			{
				header_pos.y = Mathf.Clamp(header_pos.y + diff, 0f, HeaderSize.y);
			}

			Utilities.SetTopLeftCorner(Header, header_pos);
		}

		/// <summary>
		/// Resize header.
		/// </summary>
		/// <param name="position">Current ScrollRect.content position.</param>
		protected void Resize(Vector2 position)
		{
			var size = Header.rect.size;
			var diff = IsHorizontal
				? -position.x - (-LastPosition.x)
				: position.y - LastPosition.y;
			if (IsHorizontal)
			{
				Header.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(size.x - diff, HeaderMinSize, HeaderSize.x));
			}
			else
			{
				Header.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(size.y - diff, HeaderMinSize, HeaderSize.y));
			}
		}

		/// <summary>
		/// Destroy this instance.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (ScrollRect != null)
			{
				ScrollRect.onValueChanged.RemoveListener(Scroll);
			}
		}
	}
}