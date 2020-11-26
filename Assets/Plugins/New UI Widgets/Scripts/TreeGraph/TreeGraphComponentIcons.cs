namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TreeGraph component with icons.
	/// </summary>
	public class TreeGraphComponentIcons : TreeGraphComponent<TreeViewItem>, IUpgradeable
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with NameAdapter.")]
		public Text Name;

		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public TextAdapter NameAdapter;

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
					graphicsForeground = new Graphic[] { Utilities.GetGraphic(NameAdapter), };
				}

				return graphicsForeground;
			}
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="node">Node.</param>
		public override void SetData(TreeNode<TreeViewItem> node)
		{
			Node = node;

			NameAdapter.text = node.Item.LocalizedName ?? node.Item.Name;

			name = node.Item.Name;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Name, ref NameAdapter);
#pragma warning restore 0612, 0618
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected virtual void OnValidate()
		{
			if (!Compatibility.IsPrefab(this))
			{
				Upgrade();
			}
		}
#endif

	}
}