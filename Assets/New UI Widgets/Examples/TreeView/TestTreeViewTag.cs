namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test TreeView tag.
	/// </summary>
	public class TestTreeViewTag : MonoBehaviour
	{
		/// <summary>
		/// TreeView.
		/// </summary>
		[SerializeField]
		public TreeView Tree;

		/// <summary>
		/// Starts this instance.
		/// </summary>
		public void Start()
		{
			// Set nodes with specified tag
			Tree.Nodes[0].Item.Tag = GameObject.Find("Test GameObject");

			// Add callbacks
			Tree.NodeSelected.AddListener(OnSelect);
			Tree.NodeDeselected.AddListener(OnDeselect);
		}

		void OnSelect(TreeNode<TreeViewItem> node)
		{
			var go = node.Item.Tag as GameObject;
			if (go != null)
			{
				go.SetActive(true);
			}
		}

		void OnDeselect(TreeNode<TreeViewItem> node)
		{
			var go = node.Item.Tag as GameObject;
			if (go != null)
			{
				go.SetActive(false);
			}
		}
	}
}