namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test Accordion.
	/// </summary>
	public class AccordionTestToggle : MonoBehaviour
	{
		/// <summary>
		/// Accordion.
		/// </summary>
		[SerializeField]
		protected Accordion TestAccordion;

		/// <summary>
		/// Toggleable GameObject.
		/// </summary>
		[SerializeField]
		protected GameObject ToggleGameObject;

		/// <summary>
		/// Simple test. Get item and open/close/toggle it.
		/// </summary>
		public void Test()
		{
			// find required item by toggle object
			var item = TestAccordion.DataSource.Find(x => x.ToggleObject == ToggleGameObject);

			// or content object
			// var item = TestAccordion.Items.Find(x => x.ContentObject == ContentGameObject);
			// or get item by index
			// var item = TestAccordion.Items[0];

			// and expand item
			TestAccordion.Open(item);

			// or close
			// TestAccordion.Close(item);
			// or toggle
			// TestAccordion.ToggleItem(item);
		}

		/// <summary>
		/// Toggle with OnClick.Invoke();
		/// </summary>
		public void SimpleToggle()
		{
			var component = ToggleGameObject.GetComponent<AccordionItemComponent>();
			component.OnClick.Invoke();
		}
	}
}