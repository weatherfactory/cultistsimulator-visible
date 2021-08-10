namespace UIWidgets
{
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewDropIndicator.
	/// </summary>
	public class ListViewDropIndicator : MonoBehaviour, IStylable
	{
		LayoutElement layoutElement;

		/// <summary>
		/// Gets the layout element.
		/// </summary>
		/// <value>The layout element.</value>
		public LayoutElement LayoutElement
		{
			get
			{
				if (layoutElement == null)
				{
					layoutElement = Utilities.GetOrAddComponent<LayoutElement>(this);
				}

				return layoutElement;
			}
		}

		/// <summary>
		/// Show indicator for the specified index in listView.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="listView">ListView.</param>
		public virtual void Show(int index, ListViewBase listView)
		{
			if (index == -1)
			{
				Hide();
				return;
			}

			var size = listView.IsHorizontal() ? listView.GetDefaultItemHeight() : listView.GetDefaultItemWidth();
			var p = listView.GetItemPosition(index);
			var pos = listView.IsHorizontal() ? new Vector2(p, 0f) : new Vector2(0f, -p);

			var rectTransform = transform as RectTransform;
			if (listView.IsHorizontal())
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 2f);
			}
			else
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2f);
			}

			var items_per_block = listView.GetItemsPerBlock();

			if (items_per_block == 1)
			{
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				if (listView.IsHorizontal())
				{
					rectTransform.anchorMin = new Vector2(0f, 0.5f);
					rectTransform.anchorMax = new Vector2(0f, 0.5f);
				}
				else
				{
					rectTransform.anchorMin = new Vector2(0.5f, 1f);
					rectTransform.anchorMax = new Vector2(0.5f, 1f);
				}
			}
			else
			{
				var index_in_block = index % items_per_block;

				if (listView.IsHorizontal())
				{
					rectTransform.pivot = new Vector2(0f, 1f);
					rectTransform.anchorMin = new Vector2(0f, 1f);
					rectTransform.anchorMax = new Vector2(0f, 1f);
					pos.y = -index_in_block * (listView.GetDefaultItemHeight() + listView.GetItemSpacingY());
				}
				else
				{
					rectTransform.pivot = new Vector2(0f, 0f);
					rectTransform.anchorMin = new Vector2(0f, 1f);
					rectTransform.anchorMax = new Vector2(0f, 1f);
					pos.x = index_in_block * (listView.GetDefaultItemWidth() + listView.GetItemSpacingX());
				}
			}

			rectTransform.anchoredPosition = pos;
			rectTransform.SetParent(listView.Container, false);
			rectTransform.SetAsLastSibling();

			LayoutElement.ignoreLayout = true;

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Hide indicator.
		/// </summary>
		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}

		#region IStylable implementation

		/// <inheritdoc/>
		public virtual bool SetStyle(Style style)
		{
			style.DropIndicator.Image.ApplyTo(GetComponent<Image>());

			return true;
		}

		/// <inheritdoc/>
		public virtual bool GetStyle(Style style)
		{
			style.DropIndicator.Image.GetFrom(GetComponent<Image>());

			return true;
		}
		#endregion
	}
}