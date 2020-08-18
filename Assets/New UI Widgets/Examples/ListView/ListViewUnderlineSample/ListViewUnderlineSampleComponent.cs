namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewUnderline sample component.
	/// </summary>
	public class ListViewUnderlineSampleComponent : ListViewItem, IViewData<ListViewUnderlineSampleItemDescription>
	{
		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				return new Graphic[] { Utilities.GetGraphic(TextAdapter), Underline, };
			}
		}

		/// <summary>
		/// Background graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsBackground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Icon.
		/// </summary>
		[SerializeField]
		public Image Icon;

		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with TextAdapter.")]
		public Text Text;

		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		public TextAdapter TextAdapter;

		/// <summary>
		/// Underline.
		/// </summary>
		[SerializeField]
		public Image Underline;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(ListViewUnderlineSampleItemDescription item)
		{
			Icon.sprite = item.Icon;
			TextAdapter.text = item.Name;

			Icon.SetNativeSize();

			Icon.color = (Icon.sprite == null) ? Color.clear : Color.white;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Text, ref TextAdapter);
#pragma warning restore 0612, 0618
		}
	}
}