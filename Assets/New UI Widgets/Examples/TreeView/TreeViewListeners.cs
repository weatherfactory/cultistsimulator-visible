namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test TreeView listeners.
	/// </summary>
	public class TreeViewListeners : MonoBehaviour
	{
		/// <summary>
		/// TreeView.
		/// </summary>
		public TreeView Tree;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			Tree.NodeSelected.AddListener(NodeSelected);
			Tree.NodeDeselected.AddListener(NodeDeselected);
		}

		/// <summary>
		/// Handle node selected event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void NodeSelected(TreeNode<TreeViewItem> node)
		{
			Debug.Log(node.Item.Name + " selected");
		}

		/// <summary>
		/// Handle node deselected event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void NodeDeselected(TreeNode<TreeViewItem> node)
		{
			Debug.Log(node.Item.Name + " deselected");
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Tree != null)
			{
				Tree.NodeSelected.RemoveListener(NodeSelected);
				Tree.NodeDeselected.RemoveListener(NodeDeselected);
			}
		}
	}
}