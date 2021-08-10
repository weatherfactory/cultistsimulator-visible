namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Accordion item.
	/// </summary>
	[Serializable]
	public class AccordionItem
	{
		/// <summary>
		/// The toggle object.
		/// </summary>
		public GameObject ToggleObject;

		/// <summary>
		/// The content object.
		/// </summary>
		public GameObject ContentObject;

		/// <summary>
		/// Default state of content object.
		/// </summary>
		public bool Open;

		/// <summary>
		/// The current coroutine.
		/// </summary>
		[HideInInspector]
		public Coroutine CurrentCoroutine;

		/// <summary>
		/// The content object RectTransform.
		/// </summary>
		[HideInInspector]
		public RectTransform ContentObjectRect;

		/// <summary>
		/// The content LayoutElement.
		/// </summary>
		[HideInInspector]
		public LayoutElement ContentLayoutElement;

		/// <summary>
		/// The height of the content object.
		/// </summary>
		[HideInInspector]
		public float ContentObjectHeight;

		/// <summary>
		/// The width of the content object.
		/// </summary>
		[HideInInspector]
		public float ContentObjectWidth;
	}
}