namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewInt base component.
	/// </summary>
	public class ListViewIntComponentBase : ListViewItem, IViewData<int>
	{
		/// <summary>
		/// The number.
		/// </summary>
		[SerializeField]
		public TextAdapter NumberAdapter;

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
					graphicsForeground = new Graphic[] { Utilities.GetGraphic(NumberAdapter), };
				}

				return graphicsForeground;
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(int item)
		{
			NumberAdapter.text = item.ToString();
		}
	}
}