namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

	/// <summary>
	/// Table cell.
	/// </summary>
	public class TableCell : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
	{
		/// <summary>
		/// CellClicked event.
		/// </summary>
		public UnityEvent CellClicked = new UnityEvent();

		/// <summary>
		/// Handle pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnPointerClick(PointerEventData eventData)
		{
			CellClicked.Invoke();
		}

		/// <summary>
		/// Handle pointer up event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnPointerUp(PointerEventData eventData)
		{
		}

		/// <summary>
		/// Handle pointer down event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnPointerDown(PointerEventData eventData)
		{
		}
	}
}