namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// TreeViewCustom drop support.
	/// </summary>
	/// <typeparam name="TTreeView">Type of the TreeView.</typeparam>
	/// <typeparam name="TTreeViewComponent">Type of the TreeView component.</typeparam>
	/// <typeparam name="TTreeViewItem">Type of the TreeView item.</typeparam>
	public class TreeViewCustomDropSupport<TTreeView, TTreeViewComponent, TTreeViewItem> : MonoBehaviour, IDropSupport<TreeNode<TTreeViewItem>>, IDropSupport<TTreeViewItem>
		where TTreeView : TreeViewCustom<TTreeViewComponent, TTreeViewItem>
		where TTreeViewComponent : TreeViewComponentBase<TTreeViewItem>
	{
		TTreeView source;

		/// <summary>
		/// Gets the current TreeView.
		/// </summary>
		/// <value>Current TreeView.</value>
		public TTreeView Source
		{
			get
			{
				if (source == null)
				{
					source = GetComponent<TTreeView>();
				}

				return source;
			}
		}

		/// <summary>
		/// Receive dropped items.
		/// </summary>
		[SerializeField]
		public bool ReceiveItems = true;

		/// <summary>
		/// Receive dropped nodes.
		/// </summary>
		[SerializeField]
		public bool ReceiveNodes = true;

		#region IDropSupport<TreeNode<TTreeViewItem>>

		/// <summary>
		/// Determines whether this instance can receive drop with the specified data and eventData.
		/// </summary>
		/// <returns><c>true</c> if this instance can receive drop with the specified data and eventData; otherwise, <c>false</c>.</returns>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public bool CanReceiveDrop(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
			if (!Source.IsInteractable())
			{
				return false;
			}

			if (!ReceiveNodes)
			{
				return false;
			}

			return Source.Nodes == null || !Source.Nodes.Contains(data);
		}

		/// <summary>
		/// Handle dropped data.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public void Drop(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
			if (Source.Nodes == null)
			{
				Source.Nodes = new ObservableList<TreeNode<TTreeViewItem>>();
			}

			Source.Nodes.Add(data);
		}

		/// <summary>
		/// Handle canceled drop.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public void DropCanceled(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
		}
		#endregion

		#region IDropSupport<TTreeViewItem>

		/// <summary>
		/// Determines whether this instance can receive drop with the specified data and eventData.
		/// </summary>
		/// <returns><c>true</c> if this instance can receive drop with the specified data and eventData; otherwise, <c>false</c>.</returns>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public bool CanReceiveDrop(TTreeViewItem data, PointerEventData eventData)
		{
			if (!Source.IsInteractable())
			{
				return false;
			}

			return ReceiveItems;
		}

		/// <summary>
		/// Handle dropped data.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public void Drop(TTreeViewItem data, PointerEventData eventData)
		{
			if (Source.Nodes == null)
			{
				Source.Nodes = new ObservableList<TreeNode<TTreeViewItem>>();
			}

			var newNode = new TreeNode<TTreeViewItem>(data, null, true, true);
			Source.Nodes.Add(newNode);
		}

		/// <summary>
		/// Handle canceled drop.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public void DropCanceled(TTreeViewItem data, PointerEventData eventData)
		{
		}
		#endregion
	}
}