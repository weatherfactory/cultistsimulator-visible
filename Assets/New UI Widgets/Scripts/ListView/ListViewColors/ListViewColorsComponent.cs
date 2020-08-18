namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewColor component.
	/// </summary>
	public class ListViewColorsComponent : ListViewItem, IViewData<Color>
	{
		/// <summary>
		/// The number.
		/// </summary>
		[SerializeField]
		public Image Color;

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Gets background graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsBackground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(Color item)
		{
			Color.color = item;
		}
	}
}