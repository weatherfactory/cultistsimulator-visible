namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewSlider component.
	/// </summary>
	public class ListViewSliderComponent : ListViewItem, IViewData<ListViewSliderItem>
	{
		/// <summary>
		/// Slider.
		/// </summary>
		public Slider Slider;

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
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(ListViewSliderItem item)
		{
			Slider.value = item.Value;
		}

		/// <summary>
		/// Handle OnMove event.
		/// Redirect left and right movements events to slider.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnMove(AxisEventData eventData)
		{
			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
				case MoveDirection.Right:
					Slider.OnMove(eventData);
					break;
				default:
					base.OnMove(eventData);
					break;
			}
		}
	}
}