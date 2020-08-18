namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewIcons item component.
	/// </summary>
	public class ListViewIconsItemComponent : ListViewItem, IViewData<ListViewIconsItemDescription>, IViewData<TreeViewItem>
	{
		GameObject[] objectsToResize;

		/// <summary>
		/// Gets the objects to resize.
		/// </summary>
		/// <value>The objects to resize.</value>
		public GameObject[] ObjectsToResize
		{
			get
			{
				if (objectsToResize == null)
				{
					objectsToResize = (TextAdapter == null)
						 ? new GameObject[] { Icon.transform.parent.gameObject }
						 : new GameObject[] { Icon.transform.parent.gameObject, TextAdapter.gameObject, };
				}

				return objectsToResize;
			}
		}

		Graphic[] graphicsForeground;

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				if (graphicsForeground == null)
				{
					graphicsForeground = new Graphic[] { Utilities.GetGraphic(TextAdapter), };
				}

				return graphicsForeground;
			}
		}

		/// <summary>
		/// The icon.
		/// </summary>
		[SerializeField]
		public Image Icon;

		/// <summary>
		/// The text.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with TextAdapter.")]
		public Text Text;

		/// <summary>
		/// The text adapter.
		/// </summary>
		[SerializeField]
		public TextAdapter TextAdapter;

		/// <summary>
		/// Set icon native size.
		/// </summary>
		public bool SetNativeSize = true;

		/// <summary>
		/// Gets the current item.
		/// </summary>
		public ListViewIconsItemDescription Item
		{
			get;
			protected set;
		}

		/// <summary>
		/// Sets component data with specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(ListViewIconsItemDescription item)
		{
			Item = item;
			name = item == null ? "DefaultItem (Clone)" : item.Name;
			if (Item == null)
			{
				if (Icon != null)
				{
					Icon.sprite = null;
				}

				if (TextAdapter != null)
				{
					TextAdapter.text = string.Empty;
				}
			}
			else
			{
				if (Icon != null)
				{
					Icon.sprite = Item.Icon;
				}

				if (TextAdapter != null)
				{
					TextAdapter.text = (Item.LocalizedName ?? Item.Name).Replace("\\n", "\n");
				}
			}

			if (Icon != null)
			{
				if (SetNativeSize)
				{
					Icon.SetNativeSize();
				}

				// set transparent color if no icon
				Icon.color = (Icon.sprite == null) ? Color.clear : Color.white;
			}
		}

		/// <summary>
		/// Sets component data with specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(TreeViewItem item)
		{
			SetData(new ListViewIconsItemDescription()
			{
				Name = item.Name,
				LocalizedName = item.LocalizedName,
				Icon = item.Icon,
				Value = item.Value,
			});
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
		/// Upgrade serialized data to the latest version.
		/// </summary>
		public override void Upgrade()
		{
			base.Upgrade();

#pragma warning disable 0618
			Utilities.GetOrAddComponent(Text, ref TextAdapter);
#pragma warning restore 0618
		}
	}
}