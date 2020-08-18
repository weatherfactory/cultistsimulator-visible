namespace UIWidgets.Examples
{
	using UIWidgets;

	/// <summary>
	/// TreeView with checkboxes.
	/// </summary>
	public class TreeViewCheckboxes : TreeViewCustom<TreeViewCheckboxesComponent, TreeViewCheckboxesItem>
	{
		/// <summary>
		/// NodeCheckboxChanged event.
		/// </summary>
		public NodeEvent OnNodeCheckboxChanged = new NodeEvent();

		void NodeCheckboxChanged(int index)
		{
			OnNodeCheckboxChanged.Invoke(DataSource[index].Node);
		}

		/// <summary>
		/// Add callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void AddCallback(ListViewItem item)
		{
			(item as TreeViewCheckboxesComponent).NodeCheckboxChanged.AddListener(NodeCheckboxChanged);
			base.AddCallback(item);
		}

		/// <summary>
		/// Remove callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void RemoveCallback(ListViewItem item)
		{
			(item as TreeViewCheckboxesComponent).NodeCheckboxChanged.RemoveListener(NodeCheckboxChanged);
			base.RemoveCallback(item);
		}
	}
}