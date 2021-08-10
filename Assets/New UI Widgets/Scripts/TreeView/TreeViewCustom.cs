namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Attributes;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// TreeViewCustom.
	/// </summary>
	/// <typeparam name="TComponent">Type of DefaultItem.</typeparam>
	/// <typeparam name="TItem">Type of item.</typeparam>
	public class TreeViewCustom<TComponent, TItem> : ListViewCustom<TComponent, ListNode<TItem>>
		where TComponent : TreeViewComponentBase<TItem>
	{
		/// <summary>
		/// NodeEvent.
		/// </summary>
		[Serializable]
		public class NodeEvent : UnityEvent<TreeNode<TItem>>
		{
		}

		#if !UNITY_2020_1_OR_NEWER
		[SerializeField]
		#endif
		ObservableList<TreeNode<TItem>> nodes;

		/// <summary>
		/// Gets or sets the nodes.
		/// </summary>
		/// <value>The nodes.</value>
		[DataBindField]
		public virtual ObservableList<TreeNode<TItem>> Nodes
		{
			get
			{
				return nodes;
			}

			set
			{
				if (!isTreeViewCustomInited)
				{
					Init();
				}

				if (nodes != null)
				{
					nodes.OnChange -= NodesChanged;
					nodes.ForEach(RemoveParent);
				}

				nodes = value;
				RootNode.Nodes = value;
				ListRenderer.SetPosition(0f);
				Refresh();

				if (nodes != null)
				{
					nodes.OnChange += NodesChanged;
				}
			}
		}

		/// <summary>
		/// Gets the selected node.
		/// </summary>
		/// <value>The selected node.</value>
		[DataBindField]
		public TreeNode<TItem> SelectedNode
		{
			get
			{
				TreeNode<TItem> result = null;

				foreach (var node in selectedNodes)
				{
					result = node;
				}

				return result;
			}
		}

		/// <summary>
		/// NodeToggle event.
		/// </summary>
		public NodeEvent NodeToggle = new NodeEvent();

		/// <summary>
		/// NodeSelected event.
		/// </summary>
		[DataBindEvent("SelectedNode", "SelectedNodes")]
		public NodeEvent NodeSelected = new NodeEvent();

		/// <summary>
		/// NodeDeselected event.
		/// </summary>
		[DataBindEvent("SelectedNode", "SelectedNodes")]
		public NodeEvent NodeDeselected = new NodeEvent();

		/// <summary>
		/// The selected nodes.
		/// </summary>
		protected HashSet<TreeNode<TItem>> selectedNodes = new HashSet<TreeNode<TItem>>();

		/// <summary>
		/// Gets or sets the selected nodes.
		/// </summary>
		/// <value>The selected nodes.</value>
		[DataBindField]
		public List<TreeNode<TItem>> SelectedNodes
		{
			get
			{
				return new List<TreeNode<TItem>>(selectedNodes);
			}

			set
			{
				selectedNodes.Clear();
				SelectedIndices = Nodes2Indices(value);
			}
		}

		[SerializeField]
		bool deselectCollapsedNodes = true;

		/// <summary>
		/// The deselect collapsed nodes.
		/// </summary>
		public bool DeselectCollapsedNodes
		{
			get
			{
				return deselectCollapsedNodes;
			}

			set
			{
				deselectCollapsedNodes = value;
				if (value)
				{
					selectedNodes.Clear();
					var items = SelectedItems;
					for (int i = 0; i < items.Count; i++)
					{
						selectedNodes.Add(items[i].Node);
					}
				}
			}
		}

		/// <summary>
		/// Opened nodes converted to list.
		/// </summary>
		protected ObservableList<ListNode<TItem>> NodesList = new ObservableList<ListNode<TItem>>();

		/// <summary>
		/// Scroll with node indent.
		/// </summary>
		[SerializeField]
		public bool ScrollWithIndent = false;

		[NonSerialized]
		bool isTreeViewCustomInited = false;

		TreeNode<TItem> rootNode;

		/// <summary>
		/// Gets the root node.
		/// </summary>
		/// <value>The root node.</value>
		protected TreeNode<TItem> RootNode
		{
			get
			{
				return rootNode;
			}
		}

		/// <summary>
		/// List can be looped and items is enough to make looped list.
		/// </summary>
		/// <value><c>true</c> if looped list available; otherwise, <c>false</c>.</value>
		public override bool LoopedListAvailable
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isTreeViewCustomInited)
			{
				return;
			}

			if (DefaultItem != null)
			{
				var default_item_layout = DefaultItem.GetComponent<EasyLayoutNS.EasyLayout>();

				if ((default_item_layout != null) && !IsHorizontal())
				{
					default_item_layout.CompactConstraint = EasyLayoutNS.CompactConstraints.MaxRowCount;
					default_item_layout.CompactConstraintCount = 1;
				}
			}

			isTreeViewCustomInited = true;

			var valid_list_type = (listType == ListViewType.ListViewWithFixedSize)
				|| (listType == ListViewType.ListViewWithVariableSize);
			if (!valid_list_type)
			{
				Debug.LogWarning("TreeView does not support TileView mode", this);
				listType = ListViewType.ListViewWithFixedSize;
			}

			rootNode = new TreeNode<TItem>(default(TItem));

			setContentSizeFitter = false;

			base.Init();

			Refresh();

			KeepSelection = true;

			DataSource = NodesList;
		}

		/// <summary>
		/// Require EasyLayout.
		/// </summary>
		protected override bool RequireEasyLayout
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Get the rendered of the specified ListView type.
		/// </summary>
		/// <param name="type">ListView type</param>
		/// <returns>Renderer.</returns>
		protected override ListViewTypeBase GetRenderer(ListViewType type)
		{
			switch (type)
			{
				case ListViewType.ListViewWithFixedSize:
					return new ListViewTypeFixed(this);
				case ListViewType.ListViewWithVariableSize:
					return new ListViewTypeSize(this);
				case ListViewType.TileViewWithFixedSize:
					throw new NotSupportedException("ListViewType.TileViewWithFixedSize unsupported for TreeView");
				case ListViewType.TileViewWithVariableSize:
					throw new NotSupportedException("ListViewType.TileViewWithVariableSize unsupported for TreeView");
				default:
					throw new NotSupportedException("Unknown ListView type: " + type);
			}
		}

		/// <summary>
		/// Sets the direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		protected override void SetDirection(ListViewDirection newDirection)
		{
			direction = newDirection;

			(Container as RectTransform).anchoredPosition = Vector2.zero;

			if (ListRenderer.IsVirtualizationSupported())
			{
				LayoutBridge.IsHorizontal = IsHorizontal();

				CalculateMaxVisibleItems();
			}

			UpdateView();
		}

		/// <summary>
		/// Convert nodes tree to list.
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="sourceNodes">Source nodes.</param>
		/// <param name="depth">Depth.</param>
		/// <param name="list">List.</param>
		protected virtual int Nodes2List(IObservableList<TreeNode<TItem>> sourceNodes, int depth, ObservableList<ListNode<TItem>> list)
		{
			var added_nodes = 0;
			for (var i = 0; i < sourceNodes.Count; i++)
			{
				var node = sourceNodes[i];
				if (!node.IsVisible)
				{
					node.Index = -1;
					ResetNodesIndex(node.Nodes);
					continue;
				}

				list.Add(new ListNode<TItem>(node, depth));
				node.Index = list.Count - 1;

				if (node.IsExpanded && (node.Nodes != null) && (node.Nodes.Count > 0))
				{
					var used = Nodes2List(node.Nodes, depth + 1, list);
					node.UsedNodesCount = used;
				}
				else
				{
					ResetNodesIndex(node.Nodes);
					node.UsedNodesCount = 0;
				}

				added_nodes += 1;
			}

			return added_nodes;
		}

		/// <summary>
		/// Reset nodes indexes.
		/// </summary>
		/// <param name="sourceNodes">Source nodes.</param>
		protected virtual void ResetNodesIndex(IObservableList<TreeNode<TItem>> sourceNodes)
		{
			if (sourceNodes == null)
			{
				return;
			}

			for (var i = 0; i < sourceNodes.Count; i++)
			{
				var node = sourceNodes[i];
				node.Index = -1;
				ResetNodesIndex(node.Nodes);
			}
		}

		/// <summary>
		/// Process the toggle node event.
		/// </summary>
		/// <param name="index">Index.</param>
		protected void OnToggleNode(int index)
		{
			ToggleNode(index);
			NodeToggle.Invoke(NodesList[index].Node);
		}

		/// <summary>
		/// Expand the parent nodes.
		/// </summary>
		/// <param name="node">Node.</param>
		public virtual void ExpandParentNodes(TreeNode<TItem> node)
		{
			var current = node.Parent;
			while (current != null)
			{
				current.IsExpanded = true;
				current = current.Parent;
			}
		}

		/// <summary>
		/// Collapse the parent nodes.
		/// </summary>
		/// <param name="node">Node.</param>
		public virtual void CollapseParentNodes(TreeNode<TItem> node)
		{
			var current = node.Parent;
			while (current != null)
			{
				current.IsExpanded = false;
				current = current.Parent;
			}
		}

		/// <summary>
		/// Select the node.
		/// </summary>
		/// <param name="node">Node.</param>
		public void Select(TreeNode<TItem> node)
		{
			if (!IsNodeInTree(node))
			{
				return;
			}

			var index = Node2Index(node);
			if (IsValid(index))
			{
				Select(index);
			}
			else if (index == -1 && !DeselectCollapsedNodes)
			{
				selectedNodes.Add(node);
			}
		}

		/// <summary>
		/// Select the node.
		/// </summary>
		/// <param name="node">Node.</param>
		public void SelectNode(TreeNode<TItem> node)
		{
			Select(node);
		}

		/// <summary>
		/// Select the node with sub-nodes.
		/// </summary>
		/// <param name="node">Node.</param>
		public void SelectNodeWithSubnodes(TreeNode<TItem> node)
		{
			if (!IsNodeInTree(node))
			{
				return;
			}

			var queue = new Queue<TreeNode<TItem>>();
			queue.Enqueue(node);

			while (queue.Count > 0)
			{
				var current_node = queue.Dequeue();

				var index = Node2Index(current_node);

				if (index != -1)
				{
					Select(index);
					if (current_node.Nodes != null)
					{
						current_node.Nodes.ForEach(queue.Enqueue);
					}
				}
				else if (!DeselectCollapsedNodes)
				{
					selectedNodes.Add(current_node);
					if (current_node.Nodes != null)
					{
						current_node.Nodes.ForEach(queue.Enqueue);
					}
				}
			}
		}

		/// <summary>
		/// Deselect the node.
		/// </summary>
		/// <param name="node">Node.</param>
		public void Deselect(TreeNode<TItem> node)
		{
			if (!IsNodeInTree(node))
			{
				return;
			}

			var index = Node2Index(node);
			if (IsValid(index))
			{
				Deselect(index);
			}
			else if (index == -1 && !DeselectCollapsedNodes)
			{
				selectedNodes.Remove(node);
			}
		}

		/// <summary>
		/// Deselect the node.
		/// </summary>
		/// <param name="node">Node.</param>
		public void DeselectNode(TreeNode<TItem> node)
		{
			Deselect(node);
		}

		/// <summary>
		/// Deselect the node with sub-nodes.
		/// </summary>
		/// <param name="node">Node.</param>
		public void DeselectNodeWithSubnodes(TreeNode<TItem> node)
		{
			if (!IsNodeInTree(node))
			{
				return;
			}

			var queue = new Queue<TreeNode<TItem>>();
			queue.Enqueue(node);

			while (queue.Count > 0)
			{
				var current_node = queue.Dequeue();

				var index = Node2Index(current_node);

				if (index != -1)
				{
					Deselect(index);
					if (current_node.Nodes != null)
					{
						current_node.Nodes.ForEach(queue.Enqueue);
					}
				}
				else if (!DeselectCollapsedNodes)
				{
					selectedNodes.Remove(current_node);
					if (current_node.Nodes != null)
					{
						current_node.Nodes.ForEach(queue.Enqueue);
					}
				}
			}
		}

		/// <inheritdoc/>
		protected override void InvokeSelect(int index, bool raiseEvents)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			var node = NodesList[index].Node;

			selectedNodes.Add(node);

			base.InvokeSelect(index, raiseEvents);

			if (raiseEvents)
			{
				NodeSelected.Invoke(node);
			}
		}

		/// <inheritdoc/>
		protected override void InvokeDeselect(int index, bool raiseEvents)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			var node = NodesList[index].Node;

			if (DeselectCollapsedNodes || !IsNodeInTree(node) || Node2Index(node) != -1)
			{
				selectedNodes.Remove(node);
			}

			base.InvokeDeselect(index, raiseEvents);

			if (raiseEvents)
			{
				NodeDeselected.Invoke(node);
			}
		}

		/// <summary>
		/// Recalculates the selected indices.
		/// </summary>
		/// <returns>The selected indices.</returns>
		/// <param name="newItems">New items.</param>
		protected override List<int> RecalculateSelectedIndices(ObservableList<ListNode<TItem>> newItems)
		{
			if (DeselectCollapsedNodes)
			{
				return base.RecalculateSelectedIndices(newItems);
			}
			else
			{
				var indices = new List<int>();

				foreach (var node in selectedNodes)
				{
					var index = Node2Index(newItems, node);
					if (index != -1)
					{
						indices.Add(index);
					}
				}

				return indices;
			}
		}

		/// <summary>
		/// Get node index.
		/// </summary>
		/// <param name="nodes">Nodes list.</param>
		/// <param name="node">Node.</param>
		/// <returns>Index of the node.</returns>
		protected int Node2Index(ObservableList<ListNode<TItem>> nodes, TreeNode<TItem> node)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].Node == node)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines whether specified node in tree.
		/// </summary>
		/// <returns><c>true</c> if tree contains specified node; otherwise, <c>false</c>.</returns>
		/// <param name="node">Node.</param>
		public bool IsNodeInTree(TreeNode<TItem> node)
		{
			return ReferenceEquals(RootNode, node.RootNode);
		}

		/// <summary>
		/// Toggles the node.
		/// </summary>
		/// <param name="index">Index.</param>
		protected virtual void ToggleNode(int index)
		{
			var node = NodesList[index];

			node.Node.IsExpanded = !node.Node.IsExpanded;
		}

		/// <summary>
		/// Get indices of specified nodes.
		/// </summary>
		/// <returns>The indices.</returns>
		/// <param name="targetNodes">Target nodes.</param>
		protected List<int> Nodes2Indices(List<TreeNode<TItem>> targetNodes)
		{
			var indices = new List<int>();

			for (int i = 0; i < targetNodes.Count; i++)
			{
				var node = targetNodes[i];
				if (!IsNodeInTree(node))
				{
					continue;
				}

				var index = Node2Index(node);

				if (index != -1)
				{
					indices.Add(index);
				}
			}

			return indices;
		}

		/// <summary>
		/// Get node index.
		/// </summary>
		/// <returns>The node index.</returns>
		/// <param name="node">Node.</param>
		public int Node2Index(TreeNode<TItem> node)
		{
			return node.Index;
		}

		/// <summary>
		/// Update view when node data changed.
		/// </summary>
		protected virtual void NodesChanged()
		{
			Refresh();
		}

		/// <summary>
		/// Refresh this instance.
		/// </summary>
		public virtual void Refresh()
		{
			if (nodes == null)
			{
				NodesList.Clear();

				return;
			}

			NodesList.BeginUpdate();

			var selected_nodes = SelectedNodes;
			NodesList.Clear();

			Nodes2List(nodes, 0, NodesList);

			SilentDeselect(SelectedIndices);
			var indices = Nodes2Indices(selected_nodes);

			SilentSelect(indices);

			NodesList.EndUpdate();

			if (DeselectCollapsedNodes)
			{
				selectedNodes.Clear();
				var items = SelectedItems;
				for (var i = 0; i < items.Count; i++)
				{
					selectedNodes.Add(items[i].Node);
				}
			}
		}

		/// <summary>
		/// Clear items of this instance.
		/// </summary>
		public override void Clear()
		{
			nodes.Clear();
			ListRenderer.SetPosition(0f);
		}

		/// <summary>
		/// [Not supported for TreeView] Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of added item.</returns>
		[Obsolete("Not supported for TreeView", true)]
		public virtual int Add(TItem item)
		{
			return -1;
		}

		/// <summary>
		/// [Not supported for TreeView] Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of removed TItem.</returns>
		[Obsolete("Not supported for TreeView", true)]
		public virtual int Remove(TItem item)
		{
			return -1;
		}

		/// <summary>
		/// [Not supported for TreeView] Remove item by specifieitemsex.
		/// </summary>
		/// <param name="index">Index.</param>
		public override void Remove(int index)
		{
			throw new NotSupportedException("Not supported for TreeView.");
		}

		/// <summary>
		/// [Not supported for TreeView] Set the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="allowDuplicate">If set to <c>true</c> allow duplicate.</param>
		/// <returns>Index of item.</returns>
		[Obsolete("Not supported for TreeView", true)]
		public virtual int Set(TItem item, bool allowDuplicate = true)
		{
			return -1;
		}

		/// <summary>
		/// Removes first elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <param name="match">Match.</param>
		/// <returns>true if item is successfully removed; otherwise, false.</returns>
		public bool Remove(Predicate<TreeNode<TItem>> match)
		{
			var index = nodes.FindIndex(match);
			if (index != -1)
			{
				nodes.RemoveAt(index);
				return true;
			}

			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if (node.Nodes == null)
				{
					continue;
				}

				if (Remove(node.Nodes, match))
				{
					return true;
				}
			}

			return false;
		}

		static bool Remove(ObservableList<TreeNode<TItem>> nodes, Predicate<TreeNode<TItem>> match)
		{
			var index = nodes.FindIndex(match);
			if (index != -1)
			{
				nodes.RemoveAt(index);
				return true;
			}

			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if (node.Nodes == null)
				{
					continue;
				}

				if (Remove(node.Nodes, match))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void RemoveCallback(ListViewItem item)
		{
			(item as TreeViewComponentBase<TItem>).ToggleEvent.RemoveListener(OnToggleNode);
			base.RemoveCallback(item);
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void AddCallback(ListViewItem item)
		{
			(item as TreeViewComponentBase<TItem>).ToggleEvent.AddListener(OnToggleNode);
			base.AddCallback(item);
		}

		/// <summary>
		/// Scroll to the specified node immediately.
		/// </summary>
		/// <param name="node">Node.</param>
		public virtual void ScrollTo(TreeNode<TItem> node)
		{
			ExpandParentNodes(node);
			var index = Node2Index(node);
			if (index < 0)
			{
				Debug.LogWarning("Specified node not found: node not in TreeView.Nodes or function called between .BeginUpdate() and .EndUpdate() calls");
				return;
			}

			ScrollTo(index);
		}

		/// <summary>
		/// Scroll to the specified node with animation.
		/// </summary>
		/// <param name="node">Node.</param>
		public virtual void ScrollToAnimated(TreeNode<TItem> node)
		{
			ExpandParentNodes(node);
			var index = Node2Index(node);
			if (index < 0)
			{
				Debug.LogWarning("Specified node not found: node not in TreeView.Nodes or function called between .BeginUpdate() and .EndUpdate() calls");
				return;
			}

			ScrollToAnimated(index);
		}

		/// <summary>
		/// Get node indentation.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <returns>Indentation.</returns>
		public virtual float GetNodeIndentation(int index)
		{
			if (index < 0)
			{
				return 0f;
			}

			return DataSource[index].Depth * DefaultItem.PaddingPerLevel;
		}

		/// <summary>
		/// Get secondary scroll position (for the cross direction).
		/// </summary>
		/// <param name="index">Index.</param>
		/// <returns>Secondary scroll position.</returns>
		protected override float GetScrollPositionSecondary(int index)
		{
			if (!ScrollWithIndent)
			{
				return base.GetScrollPositionSecondary(index);
			}

			if (ListRenderer.IsVirtualizationPossible())
			{
				var scroll_value = IsHorizontal() ? ScrollRectSize.y : ScrollRectSize.x;

				var item_size = ListRenderer.CalculateSize(index);
				var item_value = IsHorizontal()
					? (item_size.y + LayoutBridge.GetFullMarginY())
					: (item_size.x + LayoutBridge.GetFullMarginX());

				if (scroll_value >= item_value)
				{
					return base.GetScrollPositionSecondary(index);
				}
			}

			var value = GetNodeIndentation(index);

			return IsHorizontal() ? value : -value;
		}

		/// <summary>
		/// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire Nodes.
		/// </summary>
		/// <param name="match">The Predicate{TItem} delegate that defines the conditions of the element to search for.</param>
		/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, null.</returns>
		public TreeNode<TItem> FindNode(Predicate<TreeNode<TItem>> match)
		{
			return FindNode(Nodes, match);
		}

		/// <summary>
		/// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the specified nodes.
		/// </summary>
		/// <param name="searchNodes">Nodes to search.</param>
		/// <param name="match">The Predicate{TItem} delegate that defines the conditions of the element to search for.</param>
		/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, null.</returns>
		protected static TreeNode<TItem> FindNode(ObservableList<TreeNode<TItem>> searchNodes, Predicate<TreeNode<TItem>> match)
		{
			if (searchNodes == null)
			{
				return null;
			}

			var result = searchNodes.Find(match);

			if (result != null)
			{
				return result;
			}

			for (var i = 0; i < searchNodes.Count; i++)
			{
				var node = searchNodes[i];
				var subnode = FindNode(node.Nodes, match);

				if (subnode != null)
				{
					return subnode;
				}
			}

			return null;
		}

		static void RemoveParent(TreeNode<TItem> node)
		{
			node.Parent = null;
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected override void OnDestroy()
		{
			if (nodes != null)
			{
				nodes.OnChange -= NodesChanged;
				nodes.ForEach(RemoveParent);
			}

			base.OnDestroy();
		}
	}
}