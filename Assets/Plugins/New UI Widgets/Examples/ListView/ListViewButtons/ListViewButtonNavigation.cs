namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// ListViewButtons navigation.
	/// </summary>
	public class ListViewButtonNavigation : MonoBehaviour, IMoveHandler
	{
		/// <summary>
		/// Button index.
		/// </summary>
		[SerializeField]
		protected int ButtonIndex;

		/// <summary>
		/// Current item.
		/// </summary>
		[SerializeField]
		protected ListViewButtonsComponent Item;

		/// <summary>
		/// Process move event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnMove(AxisEventData eventData)
		{
			GameObject target = null;
			var lv = Item.Owner as ListViewButtons;

			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
					target = Item.GetPrevButton(ButtonIndex);
					break;
				case MoveDirection.Right:
					target = Item.GetNextButton(ButtonIndex);
					break;
				case MoveDirection.Up:
					if (Item.Index <= 0)
					{
						break;
					}

					var prev_item = lv.GetItemComponent(Item.Index - 1);
					if (prev_item != null)
					{
						target = prev_item.GetFirstButton();
					}

					break;
				case MoveDirection.Down:
					var next_item = lv.GetItemComponent(Item.Index + 1);
					if (next_item != null)
					{
						target = next_item.GetFirstButton();
					}

					break;
				default:
					target = null;
					break;
			}

			if (target != null)
			{
				eventData.selectedObject = target;
			}
		}
	}
}