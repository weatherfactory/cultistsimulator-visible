namespace UIWidgets
{
	using System;
	using System.ComponentModel;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Tree view item.
	/// </summary>
	[Serializable]
	public class TreeViewItem : INotifyPropertyChanged
	{
		/// <summary>
		/// OnChange event.
		/// </summary>
		[Obsolete("Use PropertyChanged.")]
		public event OnChange OnChange = Utilities.DefaultHandler;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = Utilities.DefaultPropertyHandler;

		/// <summary>
		/// The icon.
		/// </summary>
		[SerializeField]
		Sprite icon;

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public Sprite Icon
		{
			get
			{
				return icon;
			}

			set
			{
				icon = value;
				Changed("Icon");
			}
		}

		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		string name;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
				Changed("Name");
			}
		}

		[NonSerialized]
		string localizedName;

		/// <summary>
		/// The localized name.
		/// </summary>
		public string LocalizedName
		{
			get
			{
				return localizedName;
			}

			set
			{
				localizedName = value;
				Changed("LocalizedName");
			}
		}

		[SerializeField]
		[FormerlySerializedAs("_value")]
		int itemValue;

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public int Value
		{
			get
			{
				return itemValue;
			}

			set
			{
				itemValue = value;
				Changed("Value");
			}
		}

		[SerializeField]
		object tag;

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public object Tag
		{
			get
			{
				return tag;
			}

			set
			{
				tag = value;
				Changed("Tag");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.TreeViewItem"/> class.
		/// </summary>
		/// <param name="itemName">Item name.</param>
		/// <param name="itemIcon">Item icon.</param>
		public TreeViewItem(string itemName, Sprite itemIcon = null)
		{
			name = itemName;
			icon = itemIcon;
		}

		/// <summary>
		/// Raise OnChange event.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void Changed(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			#pragma warning disable 0618
			OnChange();
			#pragma warning restore 0618
		}
	}
}