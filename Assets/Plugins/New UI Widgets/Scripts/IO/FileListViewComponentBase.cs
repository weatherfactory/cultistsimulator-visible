namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// FileListViewComponentBase.
	/// </summary>
	public class FileListViewComponentBase : ListViewItem, IViewData<FileSystemEntry>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		protected TextAdapter NameAdapter;

		Graphic[] graphicsForeground;

		/// <summary>
		/// Foreground graphics.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				if (graphicsForeground == null)
				{
					graphicsForeground = new Graphic[] { Utilities.GetGraphic(NameAdapter), };
				}

				return graphicsForeground;
			}
		}

		/// <summary>
		/// Icon.
		/// </summary>
		[SerializeField]
		protected Image Icon;

		/// <summary>
		/// Directory icon.
		/// </summary>
		[SerializeField]
		protected Sprite DirectoryIcon;

		/// <summary>
		/// Current item.
		/// </summary>
		protected FileSystemEntry Item;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected override void Start()
		{
			base.Start();
			onDoubleClick.AddListener(DoubleClick);
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			onDoubleClick.RemoveListener(DoubleClick);
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(FileSystemEntry item)
		{
			Item = item;

			Icon.sprite = GetIcon(item);
			Icon.color = Icon.sprite == null ? Color.clear : Color.white;

			NameAdapter.text = item.DisplayName;
		}

		/// <summary>
		/// Get icon for specified FileSystemEntry.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Icon for specified FileSystemEntry.</returns>
		public virtual Sprite GetIcon(FileSystemEntry item)
		{
			if (item.IsDirectory)
			{
				return DirectoryIcon;
			}

			return null;
		}

		/// <summary>
		/// Handle double click event.
		/// </summary>
		/// <param name="index">Item index.</param>
		protected void DoubleClick(int index)
		{
			DoubleClick();
		}

		int doubleClickFrame = -1;

		/// <summary>
		/// Handle double click event.
		/// </summary>
		public virtual void DoubleClick()
		{
			if (doubleClickFrame == Time.frameCount)
			{
				return;
			}

			doubleClickFrame = Time.frameCount;

			var flv = Owner as FileListView;
			if (Item.IsDirectory && (flv.CurrentDirectory != Item.FullName))
			{
				flv.CurrentDirectory = Item.FullName;
			}
		}
	}
}