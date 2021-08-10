namespace UIWidgets.Examples
{
	using UIWidgets;
	using UIWidgets.Extensions;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Test PickerInt.
	/// </summary>
	public class PickerIntTest : MonoBehaviour, IUpgradeable
	{
		/// <summary>
		/// PickerInt template.
		/// </summary>
		[SerializeField]
		protected PickerInt PickerTemplate;

		/// <summary>
		/// Text component to display selected value.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with InfoAdapter.")]
		protected Text Info;

		/// <summary>
		/// Text component to display selected value.
		/// </summary>
		[SerializeField]
		protected TextAdapter InfoAdapter;

		int currentValue;

		/// <summary>
		/// Run test.
		/// </summary>
		public void Test()
		{
			// create picker from template
			var picker = PickerTemplate.Clone();

			// set values from template
			picker.ListView.DataSource = PickerTemplate.ListView.DataSource.ToObservableList();

			// or set new values
			// picker.ListView.DataSource = Utilities.CreateList(100, x => x);

			// show picker
			picker.Show(currentValue, ValueSelected, Canceled);
		}

		void ValueSelected(int value)
		{
			currentValue = value;
			Debug.Log("value: " + value);
		}

		void Canceled()
		{
			Debug.Log("canceled");
		}

		/// <summary>
		/// Run test.
		/// </summary>
		public void TestShow()
		{
			// create picker from template
			var picker = PickerTemplate.Clone();

			// set values from template
			// picker.ListView.DataSource = PickerTemplate.ListView.DataSource.ToObservableList();
			// or set new values
			picker.ListView.DataSource = UtilitiesCollections.CreateList(100, x => x);

			// show picker
			picker.Show(currentValue, ShowValueSelected, ShowCanceled);
		}

		void ShowValueSelected(int value)
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