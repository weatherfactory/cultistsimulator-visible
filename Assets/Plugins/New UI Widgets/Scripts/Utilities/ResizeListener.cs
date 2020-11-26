namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

	/// <summary>
	/// Resize listener.
	/// </summary>
	public class ResizeListener : UIBehaviour
	{
		RectTransform rectTransform;

		/// <summary>
		/// Gets the RectTransform.
		/// </summary>
		/// <value>The RectTransform.</value>
		public RectTransform RectTransform
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = transform as RectTransform;
				}

				return rectTransform;
			}
		}

		/// <summary>
		/// The OnResize event.
		/// </summary>
		public UnityEvent OnResize = new UnityEvent();

		Rect oldRect;

		/// <summary>
		/// Handle RectTransform dimensions change event.
		/// </summary>
		protected override void OnRectTransformDimensionsChange()
		{
			if (!IsActive())
			{
				return;
			}

			var newRect = RectTransform.rect;
			if (oldRect.Equals(newRect))
			{
				return;
			}

			oldRect = newRect;
			OnResize.Invoke();
		}

		/// <summary>
		/// Process enable event.
		/// </summary>
		protected override void OnEnable()
		{
			OnRectTransformDimensionsChange();
		}

		/// <summary>
		/// Process the resize event.
		/// </summary>
		public void OnResizeInvoke()
		{
			OnResize.Invoke();
		}
	}
}