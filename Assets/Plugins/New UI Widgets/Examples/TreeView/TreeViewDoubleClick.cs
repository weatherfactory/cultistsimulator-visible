namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TreeView with onDoubleClick event process in TreeView script.
	/// </summary>
	public class TreeViewDoubleClick : TreeView
	{
		/// <summary>
		/// Removes the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void RemoveCallback(ListViewItem item)
		{
			base.RemoveCallback(item);
			if (item != null)
			{
				item.onDoubleClick.RemoveListener(DoubleClickListener);
			}
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void AddCallback(ListViewItem item)
		{
			base.AddCallback(item);
			item.onDoubleClick.AddListener(DoubleClickListener);
		}

		void DoubleClickListener(int index)
		{
			var node = DataSource[index].Node;

			// var component = GetItemComponent(index);
			Debug.Log(node.Item.Name);
		}
	}
}