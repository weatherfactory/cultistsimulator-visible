namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// GroupedListViewComponentItem.
	/// </summary>
	public class GroupedListViewComponentItem : ComponentPool<GroupedListViewComponentItem>, IGroupedListViewComponent, IUpgradeable
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

		/// <summary>
		/// Is graphics colors setted at least once?
		/// </summary>
		protected bool IsColorSetted;

		/// <summary>
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>GroupedListViewComponent instance.</returns>
		public IGroupedListViewComponent IInstance(Transform parent)
		{
			return Instance(parent);
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(IGroupedListItem item)
		{
			NameAdapter.text = item.Name;
		}

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foreground">Foreground color.</param>
		/// <param name="background">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public virtual void GraphicsColoring(Color foreground, Color background, float fadeDuration)
		{
			// reset default color to white, otherwise it will look darker than specified color,
			// because actual color = Text.color * Text.CrossFadeColor
			if (!IsColorSetted)
			{
				NameAdapter.color = Color.white;
			}

			// change color instantly for first time
			var graphic = Utilities.GetGraphic(NameAdapter);
			if (graphic != null)
			{
				graphic.CrossFadeColor(foreground, IsColorSetted ? fadeDuration : 0f, true, true);
			}

			IsColorSetted = true;
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
			Compatibility.Upgrade(this);
		}
#endif
	}
}