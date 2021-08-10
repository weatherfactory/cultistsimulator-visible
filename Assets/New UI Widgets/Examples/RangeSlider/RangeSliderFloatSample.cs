namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// How to use RangeSliderFloat.
	/// </summary>
	[RequireComponent(typeof(RangeSliderFloat))]
	public class RangeSliderFloatSample : MonoBehaviour, IUpgradeable
	{
		/// <summary>
		/// Text component to display RangeSlider values.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with TextAdapter.")]
		protected Text Text;

		/// <summary>
		/// Text component to display RangeSlider values.
		/// </summary>
		[SerializeField]
		protected TextAdapter TextAdapter;

		RangeSliderFloat slider;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			slider = GetComponent<RangeSliderFloat>();
			if (slider != null)
			{
				slider.OnValuesChanged.AddListener(SliderChanged);
				SliderChanged(slider.ValueMin, slider.ValueMax);
			}
		}

		/// <summary>
		/// Handle changed values.
		/// </summary>
		/// <param name="min">Min value.</param>
		/// <param name="max">Max value.</param>
		protected virtual void SliderChanged(float min, float max)
		{
			if (TextAdapter != null)
			{
				if (slider.WholeNumberOfSteps)
				{
					TextAdapter.text = string.Format("Range: {0:000.00} - {1:000.00}; Step: {2:0.00}", min, max, slider.Step);
				}
				else
				{
					TextAdapter.text = string.Format("Range: {0:000.00} - {1:000.00}", min, max);
				}
			}
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (slider != null)
			{
				slider.OnValuesChanged.RemoveListener(SliderChanged);
			}
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Text, ref TextAdapter);
#pragma warning restore 0612, 0618
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected virtual void OnValidate()
		{
			Compatibility.Upgrade(this);
		}
#endif
	}
}