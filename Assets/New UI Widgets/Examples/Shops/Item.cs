namespace UIWidgets.Examples.Shops
{
	using System;
	using System.ComponentModel;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Item.
	/// </summary>
	[Serializable]
	public class Item : INotifyPropertyChanged
	{
		[SerializeField]
		[FormerlySerializedAs("Name")]
		string name;

		/// <summary>
		/// The name.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				if (name != value)
				{
					name = value;
					NotifyPropertyChanged("Name");
				}
			}
		}

		[SerializeField]
		int count;

		/// <summary>
		/// Gets or sets the count. -1 for infinity count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return count;
			}

			set
			{
				if (count == value)
				{
					return;
				}

				if (count == -1)
				{
					NotifyPropertyChanged("Count");
					return;
				}

				count = value;
				NotifyPropertyChanged("Count");
			}
		}

		/// <summary>
		/// Occurs when data changed.
		/// </summary>
		[Obsolete("Use PropertyChanged.")]
		public event OnChange OnChange = () => { };

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = (x, y) => { };

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.Examples.Shops.Item"/> class.
		/// </summary>
		/// <param name="itemName">Name.</param>
		/// <param name="itemCount">Count.</param>
		public Item(string itemName, int itemCount)
		{
			name = itemName;
			count = itemCount;
		}

		/// <summary>
		/// Notify property changed.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			#pragma warning disable 0618
			OnChange();
			#pragma warning restore 0618
		}
	}
}