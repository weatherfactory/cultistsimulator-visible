namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Test TimePicker.
	/// </summary>
	public class TestTimePicker : MonoBehaviour, IUpgradeable
	{
		/// <summary>
		/// DatePicker template.
		/// </summary>
		[SerializeField]
		protected TimePicker PickerTemplate;

		/// <summary>
		/// Text component to display selected value.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with InfoAdapter.")]
		protected Text Info;

		/// <summary>
		/// Text component to display selected value.
		/// </summary>
		[SerializeField]
		protected TextAdapter InfoAdapter;

		TimeSpan currentValue = DateTime.Now.TimeOfDay;

		/// <summary>
		/// Open picker and log selected value.
		/// </summary>
		public void Test()
		{
			// create picker from template
			var picker = PickerTemplate.Clone();

			// show picker
			picker.Show(currentValue, ValueSelected, Canceled);
		}

		void ValueSelected(TimeSpan value)
		{
			currentValue = value;
			Debug.Log("value: " + value);
		}

		void Canceled()
		{
			Debug.Log("canceled");
		}

		/// <summary>
		/// Open picker and display selected value.
		/// </summary>
		public void TestShow()
		{
			// create picker from template
			var picker = PickerTemplate.Clone();

			// show picker
			picker.Show(currentValue, ShowValueSelected, ShowCanceled);
		}

		void ShowValueSelected(TimeSpan value)
		{
			currentValue = value;
			InfoAdapter.text = "Value: " + value;
		}

		void ShowCanceled()
		{
			InfoAdapter.text = "Canceled";
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Info, ref InfoAdapter);
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