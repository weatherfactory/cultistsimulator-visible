namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewString component.
	/// </summary>
	public class ListViewStringItemComponent : ListViewItem, IViewData<string>
	{
		/// <summary>
		/// The Text component.
		/// </summary>
		[SerializeField]
		public TextAdapter Text;

		/// <summary>
		/// Item.
		/// </summary>
		public string Item
		{
			get;
			protected set;
		}

		Graphic[] graphicsForeground;

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				if (graphicsForeground == null)
				{
					graphicsForeground = new Graphic[] { Utilities.GetGraphic(Text), };
				}

				return graphicsForeground;
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="item">Text.</param>
		public virtual void SetData(string item)
		{
			Item = item;
			Text.Value = item.Replace("\\n", "\n");
		}
	}
}