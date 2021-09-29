namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// RectTransform Drag.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class DragRectTransform : DragSupport<RectTransform>
	{
		/// <inheritdoc/>
		protected override void InitDrag(PointerEventData eventData)
		{
			Data = transform as RectTransform;
		}
	}
}