namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// KeyValueListView item.
	/// </summary>
	public class KeyValueListViewItem : ListViewItem, IViewData<KeyValuePair<string, string>>
	{
		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with TextAdapter.")]
		public Text Text;

		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		public TextAdapter TextAdapter;

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				return new Graphic[] { Utilities.GetGraphic(TextAdapter), };
			}
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(KeyValuePair<string, string> item)
		{
			TextAdapter.text = item.Value;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Text, ref TextAdapter);
#pragma warning restore 0612, 0618
		}
	}
}