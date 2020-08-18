namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Settings for the staggered layout.
	/// </summary>
	[Serializable]
	public class EasyLayoutStaggeredSettings : INotifyPropertyChanged
	{
		[SerializeField]
		[FormerlySerializedAs("FixedBlocksCount")]
		[Tooltip("Layout with fixed amount of blocks (row or columns) instead of the flexible.")]
		private bool fixedBlocksCount;

		/// <summary>
		/// Use fixed blocks count.
		/// </summary>
		public bool FixedBlocksCount
		{
			get
			{
				return fixedBlocksCount;
			}

			set
			{
				if (fixedBlocksCount != value)
				{
					fixedBlocksCount = value;
					Changed("FixedBlocksCount");
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("BlocksCount")]
		private int blocksCount = 1;

		/// <summary>
		/// Block (row or columns) count.
		/// </summary>
		public int BlocksCount
		{
			get
			{
				return blocksCount;
			}

			set
			{
				if (blocksCount != value)
				{
					blocksCount = value;
					Changed("BlocksCount");
				}
			}
		}

		/// <summary>
		/// PaddingInner at the start of the blocks.
		/// Used by ListViewCustom to simulate the space occupied by non-displayable elements.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		public List<float> PaddingInnerStart = new List<float>();

		/// <summary>
		/// PaddingInner at the end of the blocks.
		/// Used by ListViewCustom to simulate the space occupied by non-displayable elements.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		public List<float> PaddingInnerEnd = new List<float>();

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