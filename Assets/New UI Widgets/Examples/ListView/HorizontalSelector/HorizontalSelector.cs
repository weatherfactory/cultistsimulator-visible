namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// Horizontal selector.
	/// </summary>
	public class HorizontalSelector : MonoBehaviour
	{
		/// <summary>
		/// ListView.
		/// </summary>
		[SerializeField]
		public ListViewIcons ListView;

		/// <summary>
		/// Paginator.
		/// </summary>
		[SerializeField]
		public ListViewPaginator Paginator;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected void Start()
		{
			ListView.MultipleSelect = false;
			ListView.Select(0);

			Paginator.OnPageSelect.AddListener(ListView.Select);
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected void OnDestroy()
		{
			Paginator.OnPageSelect.RemoveListener(ListView.Select);
		}
	}
}