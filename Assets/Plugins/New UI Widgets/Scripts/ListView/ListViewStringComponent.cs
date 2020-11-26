namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// List view item component.
	/// </summary>
	public class ListViewStringComponent : ListViewItem, IViewData<string>
	{
		/// <summary>
		/// The Text component.
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
		/// Sets the data.
		/// </summary>
		/// <param name="item">Text.</param>
		public virtual void SetData(string item)
		{
			TextAdapter.text = item.Replace("\\n", "\n");
		}

		/// <summary>
		/// Upgrade serialized data to the latest version.
		/// </summary>
		public override void Upgrade()
		{
			base.Upgrade();

#pragma warning disable 0618
			if (Text == null)
			{
				Text = Compatibility.GetComponentInChildren<Text>(this, true);
			}

			Utilities.GetOrAddComponent(Text, ref TextAdapter);
#pragma warning restore 0618
		}
	}
}