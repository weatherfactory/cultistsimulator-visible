namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using System.Linq;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test layout switcher.
	/// </summary>
	public class TestLayoutSwitcher : MonoBehaviour
	{
		/// <summary>
		/// Layout switcher.
		/// </summary>
		[SerializeField]
		public LayoutSwitcher Switcher;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			Switcher.LayoutSelector = Selector;
		}

		/// <summary>
		/// Example of the custom layout selector.
		/// </summary>
		/// <param name="layouts">Layouts list.</param>
		/// <param name="displaySize">Display size.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <returns>Layout.</returns>
		protected static UILayout Selector(List<UILayout> layouts, float displaySize, float aspectRatio)
		{
			// portrait mode
			if (aspectRatio < 1f)
			{
				var layout = layouts.Where(x => x.AspectRatioFloat >= aspectRatio).OrderBy(x => x.AspectRatioFloat).FirstOrDefault();
				if (layout != null)
				{
					return layout;
				}
			}

			// landscape mode
			return layouts.Where(x => x.AspectRatioFloat >= 1f).OrderBy(x => Mathf.Abs(aspectRatio - x.AspectRatioFloat)).FirstOrDefault();
		}

		/// <summary>
		/// Select 4:3 layout only for 4:3, in other cases use 16:9 layout.
		/// </summary>
		/// <param name="layouts">Layouts list.</param>
		/// <param name="displaySize">Display size.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <returns>Layout.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "displaySize", Justification = "Interface compatibility.")]
		protected static UILayout Selector43(List<UILayout> layouts, float displaySize, float aspectRatio)
		{
			// if aspect_ration 4:3
			if (Mathf.Approximately(4f / 3f, aspectRatio))
			{
				var layout_4_3_index = 0;
				return layouts[layout_4_3_index];
			}

			var layout_9_16_index = 1;
			return layouts[layout_9_16_index];
		}
	}
}