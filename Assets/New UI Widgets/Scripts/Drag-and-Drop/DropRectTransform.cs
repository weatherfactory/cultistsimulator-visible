namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// RectTransform drop component.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class DropRectTransform : MonoBehaviour, IDropSupport<RectTransform>
	{
		/// <inheritdoc/>
		public void Drop(RectTransform data, PointerEventData eventData)
		{
			data.SetParent(transform, false);
		}

		/// <inheritdoc/>
		public bool CanReceiveDrop(RectTransform data, PointerEventData eventData)
		{
			return data.parent != transform;
		}

		/// <inheritdoc/>
		public void DropCanceled(RectTransform data, PointerEventData eventData)
		{
		}
	}
}