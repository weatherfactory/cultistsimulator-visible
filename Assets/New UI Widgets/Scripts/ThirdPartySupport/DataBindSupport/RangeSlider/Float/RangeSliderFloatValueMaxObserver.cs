#if UIWIDGETS_DATABIND_SUPPORT
namespace UIWidgets.DataBind
{
	using Slash.Unity.DataBind.Foundation.Observers;

	/// <summary>
	/// Observes value changes of the ValueMax of an RangeSliderFloat.
	/// </summary>
	public class RangeSliderFloatValueMaxObserver : ComponentDataObserver<UIWidgets.RangeSliderFloat, float>
	{
		/// <inheritdoc />
		protected override void AddListener(UIWidgets.RangeSliderFloat target)
		{
			target.OnValuesChange.AddListener(OnValuesChangeRangeSliderFloat);
		}

		/// <inheritdoc />
		protected override float GetValue(UIWidgets.RangeSliderFloat target)
		{
			return target.ValueMax;
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