namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// GroupedListViewComponent.
	/// </summary>
	public class GroupedTileViewComponent : ListViewItem, IViewData<Photo>
	{
		/// <summary>
		/// Date.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with DateAdapter.")]
		public Text Date;

		/// <summary>
		/// Date.
		/// </summary>
		[SerializeField]
		public TextAdapter DateAdapter;

		/// <summary>
		/// Image.
		/// </summary>
		[SerializeField]
		public Image Image;

		/// <summary>
		/// Displayed item.
		/// </summary>
		protected Photo Item;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(Photo item)
		{
			Item = item;

			var height = Item.IsGroup ? 25f : 80f;
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			if (Item.IsEmpty)
			{
				Image.gameObject.SetActive(false);
				DateAdapter.gameObject.SetActive(false);
			}
			else
			{
				if (Item.IsGroup)
				{
					Image.sprite = null;
					Image.gameObject.SetActive(false);
					DateAdapter.gameObject.SetActive(true);

					DateAdapter.text = Item.Created.ToString("MMM. dd, yyyy");
				}
				else
				{
					Image.gameObject.SetActive(true);
					DateAdapter.gameObject.SetActive(false);

					Image.sprite = Item.Image;
					Image.color = Color.white;
				}
			}
		}

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public override void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration = 0f)
		{
			// disable coloring
		}

		/// <summary>
		/// Called when item moved to cache, you can use it free used resources.
		/// </summary>
		public override void MovedToCache()
		{
			Image.sprite = null;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Date, ref DateAdapter);
#pragma warning restore 0612, 0618
		}
	}
}