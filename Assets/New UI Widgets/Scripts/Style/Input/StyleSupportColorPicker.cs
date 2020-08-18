namespace UIWidgets.Styles
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Style support for the ColorPicker.
	/// </summary>
	public class StyleSupportColorPicker : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Style support for the palette toggle.
		/// </summary>
		[SerializeField]
		public StyleSupportButton PaletteToggle;

		/// <summary>
		/// Style support for the input toggle.
		/// </summary>
		[SerializeField]
		public StyleSupportButton InputToggle;

		/// <summary>
		/// The input channels labels.
		/// </summary>
		public List<GameObject> InputChannelLabels;

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			style.ColorPicker.PaletteToggle.ApplyTo(PaletteToggle);
			style.ColorPicker.InputToggle.ApplyTo(InputToggle);

			for (int i = 0; i < InputChannelLabels.Count; i++)
			{
				style.ColorPicker.InputChannelLabel.ApplyTo(InputChannelLabels[i]);
			}

			return true;
		}
		#endregion
	}
}