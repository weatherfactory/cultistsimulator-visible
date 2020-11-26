namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewIconsItem extended component.
	/// </summary>
	public class ListViewComponentCustomColoring : ListViewIconsItemComponent
	{
		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				// disable default coloring for the text
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Color for the Text if Index is even.
		/// </summary>
		public Color ColorEven = Color.black;

		/// <summary>
		/// Color for the Text if Index is odd.
		/// </summary>
		public Color ColorOdd = Color.white;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public override void SetData(ListViewIconsItemDescription item)
		{
			base.SetData(item);

			TextAdapter.color = (Index % 2) == 0 ? ColorEven : ColorOdd;
		}
	}
}