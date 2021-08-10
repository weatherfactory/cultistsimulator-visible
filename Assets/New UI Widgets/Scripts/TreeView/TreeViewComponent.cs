namespace UIWidgets
{
	using System.ComponentModel;
	using UIWidgets.l10n;
	using UnityEngine;

	/// <summary>
	/// TreeView component.
	/// </summary>
	public class TreeViewComponent : TreeViewComponentBase<TreeViewItem>
	{
		TreeViewItem item;

		/// <summary>
		/// Gets or sets the item.
		/// </summary>
		/// <value>The item.</value>
		public TreeViewItem Item
		{
			get
			{
				return item;
			}

			set
			{
				if (item != null)
				{
					item.PropertyChanged -= UpdateView;
				}

				item = value;

				if (item != null)
				{
					item.PropertyChanged += UpdateView;
				}

				UpdateView();
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="depth">Depth.</param>
		public override void SetData(TreeNode<TreeViewItem> node, int depth)
		{
			Node = node;
			base.SetData(Node, depth);

			Item = (Node == null) ? null : Node.Item;
		}

		/// <inheritdoc/>
		public override void LocaleChanged()
		{
			UpdateName();
		}

		/// <summary>
		/// Update display name.
		/// </summary>
		protected virtual void UpdateName()
		{
			if (Item == null)
			{
				return;
			}

			TextAdapter.text = Item.LocalizedName ?? Localization.GetTranslation(Item.Name);
		}

		/// <summary>
		/// Updates the view.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="ev">Event.</param>
		protected virtual void UpdateView(object sender = null, PropertyChangedEventArgs ev = null)
		{
			if ((Icon == null) || (TextAdapter == null))
			{
				return;
			}

			if (Item == null)
			{
				Icon.sprite = null;
				TextAdapter.text = string.Empty;
			}
			else
			{
				Icon.sprite = Item.Icon;
				UpdateName();
			}

			if (SetNativeSize)
			{
				Icon.SetNativeSize();
			}

			// set transparent color if no icon
			Icon.color = (Icon.sprite == null) ? Color.clear : Color.white;
		}

		/// <summary>
		/// Called when item moved to cache, you can use it free used resources.
		/// </summary>
		public override void MovedToCache()
		{
			if (Icon != null)
			{
				Icon.sprite = null;
			}
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			if (item != null)
			{
				item.PropertyChanged -= UpdateView;
			}

			base.OnDestroy();
		}
	}
}