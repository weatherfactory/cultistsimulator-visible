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
		/// Content GameObject.
		/// </summary>
		[SerializeField]
		protected GameObject ContentGameObject;

		/// <summary>
		/// Simple test. Get item and open it.
		/// </summary>
		public void Test()
		{
			// find required item by toggle object
			var item = TestAccordion.DataSource.Find(x => x.ToggleObject == ToggleGameObject);

			// and expand item
			TestAccordion.Open(item);
		}

		/// <summary>
		/// Get item and close it.
		/// </summary>
		public void TestClose()
		{
			// find required item by content object
			var item = TestAccordion.DataSource.Find(x => x.ContentObject == ContentGameObject);

			// and close it
			TestAccordion.Close(item);
		}

		/// <summary>
		/// Get item and toggle it.
		/// </summary>
		public void TestToggle()
		{
			// get item by index
			var item = TestAccordion.DataSource[0];

			// and toggle item
			TestAccordion.ToggleItem(item);
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