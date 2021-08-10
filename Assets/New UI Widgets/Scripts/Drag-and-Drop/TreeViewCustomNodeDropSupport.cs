namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// TreeViewCustomNode drop support.
	/// </summary>
	/// <typeparam name="TTreeView">Type of the TreeView.</typeparam>
	/// <typeparam name="TTreeViewComponent">Type of the TreeView component.</typeparam>
	/// <typeparam name="TTreeViewItem">Type of the TreeView item.</typeparam>
	public class TreeViewCustomNodeDropSupport<TTreeView, TTreeViewComponent, TTreeViewItem> : MonoBehaviour, IDropSupport<TreeNode<TTreeViewItem>>, IDropSupport<TTreeViewItem>
		where TTreeView : TreeViewCustom<TTreeViewComponent, TTreeViewItem>
		where TTreeViewComponent : TreeViewComponentBase<TTreeViewItem>
	{
		TTreeViewComponent source;

		/// <summary>
		/// Gets the current TreeViewComponent.
		/// </summary>
		/// <value>Current TreeViewComponent.</value>
		public TTreeViewComponent Source
		{
			get
			{
				if (source == null)
				{
					source = GetComponent<TTreeViewComponent>();
				}

				return source;
			}
		}

		/// <summary>
		/// TreeView.
		/// </summary>
		protected TTreeView TreeView
		{
			get
			{
				return Source.Owner as TTreeView;
			}
		}

		/// <summary>
		/// The drop indicator.
		/// </summary>
		[SerializeField]
		public ListViewDropIndicator DropIndicator;

		RectTransform rectTransform;

		/// <summary>
		/// Current RectTransform.
		/// </summary>
		protected RectTransform RectTransform
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = transform as RectTransform;
				}

				return rectTransform;
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

		/// <summary>
		/// Distance in percent of height from border to add dropped node before/after current instead of drop as sub-node.
		/// </summary>
		[SerializeField]
		[Tooltip("Distance in percent of height from border to add dropped node before/after instead of drop as sub-node.")]
		[Range(0f, 0.5f)]
		public float ReorderArea = 0.15f;

		/// <summary>
		/// Get drop type.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		/// <returns>Drop type.</returns>
		protected TreeNodeDropType GetDropType(PointerEventData eventData)
		{
			Vector2 point;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out point))
			{
				return TreeNodeDropType.Child;
			}

			var height = RectTransform.rect.height;
			var position = (height * (1f - RectTransform.pivot.y)) - point.y;
			var distance = height * ReorderArea;

			if (position <= distance)
			{
				return TreeNodeDropType.Before;
			}

			if (position >= (height - distance))
			{
				return TreeNodeDropType.After;
			}

			return TreeNodeDropType.Child;
		}

		#region IDropSupport<TreeNode<TItem>>

		/// <summary>
		/// Determines whether this instance can receive drop with the specified data and eventData.
		/// </summary>
		/// <returns><c>true</c> if this instance can receive drop with the specified data and eventData; otherwise, <c>false</c>.</returns>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual bool CanReceiveDrop(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
			if (!Source.IsInteractable())
			{
				return false;
			}

			if (!ReceiveNodes)
			{
				return false;
			}

			if (ReferenceEquals(data, Source.Node))
			{
				return false;
			}

			var type = GetDropType(eventData);
			var can_drop = (type == TreeNodeDropType.Child) ? data.CanBeParent(Source.Node) : data.CanBeParent(Source.Node.Parent);

			if (!can_drop)
			{
				return false;
			}

			var index = DropIndicatorIndex(type);
			ShowDropIndicator(index);

			return true;
		}

		/// <summary>
		/// Handle dropped data.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual void Drop(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
			var type = GetDropType(eventData);

			TreeView.Nodes.BeginUpdate();

			var current = Source.Node;
			var parent = current.Parent;
			int index;

			switch (type)
			{
				case TreeNodeDropType.Child:
					current.IsExpanded = true;
					data.Parent = current;
					break;
				case TreeNodeDropType.Before:
					data.Parent = null;
					index = parent.Nodes.IndexOf(current);
					parent.Nodes.Insert(index, data);
					break;
				case TreeNodeDropType.After:
					data.Parent = null;
					index = parent.Nodes.IndexOf(current) + 1;
					parent.Nodes.Insert(index, data);
					break;
			}

			TreeView.Nodes.EndUpdate();

			HideDropIndicator();
		}

		/// <summary>
		/// Handle canceled drop.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual void DropCanceled(TreeNode<TTreeViewItem> data, PointerEventData eventData)
		{
			HideDropIndicator();
		}
		#endregion

		#region IDropSupport<TTreeViewItem>

		/// <summary>
		/// Determines whether this instance can receive drop with the specified data and eventData.
		/// </summary>
		/// <returns><c>true</c> if this instance can receive drop with the specified data and eventData; otherwise, <c>false</c>.</returns>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual bool CanReceiveDrop(TTreeViewItem data, PointerEventData eventData)
		{
			if (!Source.IsInteractable())
			{
				return false;
			}

			if (!ReceiveItems)
			{
				return false;
			}

			var type = GetDropType(eventData);
			var index = DropIndicatorIndex(type);
			ShowDropIndicator(index);

			return true;
		}

		/// <summary>
		/// Handle dropped data.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual void Drop(TTreeViewItem data, PointerEventData eventData)
		{
			var type = GetDropType(eventData);

			var new_node = new TreeNode<TTreeViewItem>(data, null, true, true);
			AddNewNode(new_node, type);

			HideDropIndicator();
		}

		/// <summary>
		/// Add new node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="type">Drop type.</param>
		protected virtual void AddNewNode(TreeNode<TTreeViewItem> node, TreeNodeDropType type)
		{
			var current = Source.Node;
			var parent = current.Parent;
			int index;

			switch (type)
			{
				case TreeNodeDropType.Child:
					if (current.Nodes == null)
					{
						current.Nodes = new ObservableList<TreeNode<TTreeViewItem>>();
					}

					current.Nodes.Add(node);
					break;
				case TreeNodeDropType.Before:
					index = parent.Nodes.IndexOf(current);
					parent.Nodes.Insert(index, node);
					break;
				case TreeNodeDropType.After:
					index = parent.Nodes.IndexOf(current) + 1;
					parent.Nodes.Insert(index, node);
					break;
			}
		}

		/// <summary>
		/// Handle canceled drop.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="eventData">Event data.</param>
		public virtual void DropCanceled(TTreeViewItem data, PointerEventData eventData)
		{
			HideDropIndicator();
		}
		#endregion

		/// <summary>
		/// Get index for the drop indicator.
		/// </summary>
		/// <param name="dropType">Drop type.</param>
		/// <returns>Index.</returns>
		protected int DropIndicatorIndex(TreeNodeDropType dropType)
		{
			switch (dropType)
			{
				case TreeNodeDropType.Before:
					return Source.Index;
				case TreeNodeDropType.After:
					return Source.Index + 1;
			}

			return -1;
		}

		/// <summary>
		/// Shows the drop indicator.
		/// </summary>
		/// <param name="index">Index.</param>
		protected virtual void ShowDropIndicator(int index)
		{
			if (DropIndicator != null)
			{
				DropIndicator.Show(index, Source.Owner);
			}
		}

		/// <summary>
		/// Hides the drop indicator.
		/// </summary>
		protected virtual void HideDropIndicator()
		{
			if (DropIndicator != null)
			{
				DropIndicator.Hide();
			}
		}
	}
}