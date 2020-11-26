namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Test ColorPickerRange.
	/// </summary>
	public class TestColorPickerRange : MonoBehaviour
	{
		/// <summary>
		/// ColorPickerRange.
		/// </summary>
		[SerializeField]
		protected ColorPickerRange ColorRange;

		/// <summary>
		/// Image component to display selected color.
		/// </summary>
		[SerializeField]
		protected Image Image;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			ColorRange.OnChange.AddListener(ColorChanged);

			ColorChanged(ColorRange.Color);
		}

		void ColorChanged(Color32 color)
		{
			Image.color = color;
		}

		/// <summary>
		/// Set gree color.
		/// </summary>
		public void TestGreen()
		{
			ColorRange.Color = Color.green;
		}

		/// <summary>
		/// Set gray color.
		/// </summary>
		public void TestGray()
		{
			ColorRange.Color = Color.gray;
		}

		/// <summary>
		/// Test set color.
		/// </summary>
		public void TestColor()
		{
			ColorRange.Color = ColorRange.Color;
		}

		/// <summary>
		/// Set custom color.
		/// </summary>
		public void TestRG()
		{
			ColorRange.Color = new Color32(100, 50, 0, 255);
		}
	}
}