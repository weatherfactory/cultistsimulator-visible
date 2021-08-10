namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test tooltip.
	/// </summary>
	public class TestTooltip : MonoBehaviour
	{
		/// <summary>
		/// The tooltip template.
		/// </summary>
		[SerializeField]
		protected GameObject TooltipTemplate;

		/// <summary>
		/// The target.
		/// </summary>
		[SerializeField]
		protected GameObject Target;

		/// <summary>
		/// Test this instance.
		/// </summary>
		public void Test()
		{
			AddTooltip(Target);
		}

		/// <summary>
		/// Adds the tooltip.
		/// </summary>
		/// <param name="target">Target.</param>
		public void AddTooltip(GameObject target)
		{
			var tooltip = target.AddComponent<Tooltip>();
			var obj = Compatibility.Instantiate(TooltipTemplate);
			obj.transform.SetParent(target.transform);
			(obj.transform as RectTransform).anchoredPosition = Vector2.zero;
			obj.SetActive(true);
			obj.GetComponentInChildren<TextAdapter>().text = "Added from script";

			tooltip.TooltipObject = obj;
		}
	}
}