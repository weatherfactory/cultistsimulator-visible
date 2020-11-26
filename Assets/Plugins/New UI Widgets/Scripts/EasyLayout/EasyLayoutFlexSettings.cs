namespace EasyLayoutNS
{
	using System;
	using System.ComponentModel;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Flex settings.
	/// </summary>
	[Serializable]
	public class EasyLayoutFlexSettings : INotifyPropertyChanged
	{
		/// <summary>
		/// Content positions.
		/// </summary>
		[Serializable]
		public enum Content
		{
			/// <summary>
			/// Position at the start of the block.
			/// </summary>
			Start = 0,

			/// <summary>
			/// Position at the center of the block.
			/// </summary>
			Center = 1,

			/// <summary>
			/// Position at the end of the block.
			/// </summary>
			End = 2,

			/// <summary>
			/// Position with space between.
			/// </summary>
			SpaceBetween = 3,

			/// <summary>
			/// Position with space around.
			/// </summary>
			SpaceAround = 4,

			/// <summary>
			/// Position with space evenly.
			/// </summary>
			SpaceEvenly = 5,
		}

		/// <summary>
		/// Items align.
		/// </summary>
		[Serializable]
		public enum Items
		{
			/// <summary>
			/// Start position.
			/// </summary>
			Start = 0,

			/// <summary>
			/// Center position.
			/// </summary>
			Center = 1,

			/// <summary>
			/// End position.
			/// </summary>
			End = 2,
		}

		[SerializeField]
		[FormerlySerializedAs("Wrap")]
		bool wrap = true;

		/// <summary>
		/// Wrap.
		/// </summary>
		public bool Wrap
		{
			get
			{
				return wrap;
			}

			set
			{
				if (wrap != value)
				{
					wrap = value;
					Changed("Wrap");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("JustifyContent")]
		Content justifyContent = Content.Start;

		/// <summary>
		/// Elements positions at the main axis.
		/// </summary>
		public Content JustifyContent
		{
			get
			{
				return justifyContent;
			}

			set
			{
				if (justifyContent != value)
				{
					justifyContent = value;
					Changed("JustifyContent");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("AlignContent")]
		Content alignContent = Content.Start;

		/// <summary>
		/// Elements positions at the sub axis.
		/// </summary>
		public Content AlignContent
		{
			get
			{
				return alignContent;
			}

			set
			{
				if (alignContent != value)
				{
					alignContent = value;
					Changed("AlignContent");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("AlignItems")]
		Items alignItems = Items.Start;

		/// <summary>
		/// Items align.
		/// </summary>
		public Items AlignItems
		{
			get
			{
				return alignItems;
			}

			set
			{
				if (alignItems != value)
				{
					alignItems = value;
					Changed("AlignItems");
				}
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = EasyLayout.DefaultPropertyHandler;

		/// <summary>
		/// Property changed.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void Changed(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}