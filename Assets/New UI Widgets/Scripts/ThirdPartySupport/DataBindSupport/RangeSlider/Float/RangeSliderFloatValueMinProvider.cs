#if UIWIDGETS_DATABIND_SUPPORT
namespace UIWidgets.DataBind
{
	using Slash.Unity.DataBind.Foundation.Providers.Getters;
	using UnityEngine;

	/// <summary>
	/// Provides the ValueMin of an RangeSliderFloat.
	/// </summary>
	[AddComponentMenu("Data Bind/New UI Widgets/Getters/[DB] RangeSliderFloat ValueMin Provider")]
	public class RangeSliderFloatValueMinProvider : ComponentDataProvider<UIWidgets.RangeSliderFloat, float>
	{
		/// <inheritdoc />
		protected override void AddListener(UIWidgets.RangeSliderFloat target)
		{
			target.OnValuesChange.AddListener(OnValuesChangeRangeSliderFloat);
		}

		/// <inheritdoc />
		protected override float GetValue(UIWidgets.RangeSliderFloat target)
		{
			return target.ValueMin;
		}

		/// <inheritdoc />
		protected override void RemoveListener(UIWidgets.RangeSliderFloat target)
		{
			target.OnValuesChange.RemoveListener(OnValuesChangeRangeSliderFloat);
		}

		void OnValuesChangeRangeSliderFloat(float arg0, float arg1)
		{
			OnTargetValueChanged();
		}
	}
}
#endif