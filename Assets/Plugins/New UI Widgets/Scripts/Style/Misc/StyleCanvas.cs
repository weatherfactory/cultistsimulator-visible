namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style for the canvas.
	/// </summary>
	[Serializable]
	public class StyleCanvas : IStyleDefaultValues
	{
		/// <summary>
		/// Style for the default background.
		/// </summary>
		[SerializeField]
		public StyleImage Background;

#if UNITY_EDITOR
		/// <summary>
		/// Sets the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			Background.SetDefaultValues();
		}
#endif

		/// <summary>
		/// Apply style to the specified slider.
		/// </summary>
		/// <param name="component">Slider.</param>
		public virtual void ApplyTo(Canvas component)
		{
			if (component == null)
			{
				return;
			}

			Background.ApplyTo(component);
		}
	}
}