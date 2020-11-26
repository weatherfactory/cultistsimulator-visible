#if UIWIDGETS_TMPRO_SUPPORT
namespace UIWidgets.TMProSupport
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TabIconActiveButtonTMPro.
	/// </summary>
	public class TabIconActiveButtonTMPro : TabIconButtonTMPro
	{
		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public override void SetData(TabIcons tab)
		{
			NameAdapter.text = tab.Name;

			if (Icon != null)
			{
				Icon.sprite = tab.IconActive;

				if (SetNativeSize)
				{
					Icon.SetNativeSize();
				}

				Icon.color = (Icon.sprite == null) ? Color.clear : Color.white;
			}
		}
	}
}
#endif