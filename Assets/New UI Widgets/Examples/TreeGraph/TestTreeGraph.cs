namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test TreeGraph.
	/// </summary>
	public class TestTreeGraph : MonoBehaviour
	{
		/// <summary>
		/// Graph.
		/// </summary>
		[SerializeField]
		public TreeGraph Graph;

		/// <summary>
		/// Nodes.
		/// </summary>
		protected ObservableList<TreeNode<TreeViewItem>> Nodes;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Graph.Init();

			Graph.Nodes = Nodes;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestTreeGraph"/> class.
		/// </summary>
		public TestTreeGraph()
		{
			var config = new List<int>() { 1, 3, 2, };

			Nodes = GenerateTreeNodes(config, isExpanded: true);
		}

		/// <summary>
		/// Generate tree nodes.
		/// </summary>
		/// <param name="items">Depth list.</param>
		/// <param name="nameStartsWith">Start part of name.</param>
		/// <param name="isExpanded">Is nodes expanded?</param>
		/// <param name="start">Start index in the depth list.</param>
		/// <returns>Nodes.</returns>
		public static ObservableList<TreeNode<TreeViewItem>> GenerateTreeNodes(List<int> items, string nameStartsWith = "Node ", bool isExpanded = true, int start = 0)
		{
			return UtilitiesCollections.CreateList(items[start], x =>
			{
				var item_name = nameStartsWith + x;
				var item = new TreeViewItem(item_name, null);
				var nodes = items.Count > (start + 1)
					? GenerateTreeNodes(items, item_name + " - ", isExpanded, start + 1)
					: null;

				return new TreeNode<TreeViewItem>(item, nodes, isExpanded);
			});
		}
	}
}