namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TreeViewSample component continent.
	/// </summary>
	public class TreeViewSampleComponentContinent : ComponentPool<TreeViewSampleComponentContinent>, ITreeViewSampleMultipleComponent, IUpgradeable
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
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>GroupedListViewComponent instance.</returns>
		public ITreeViewSampleMultipleComponent IInstance(Transform parent)
		{
			return Instance(parent);
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(ITreeViewSampleItem item)
		{
			SetData(item as TreeViewSampleItemContinent);
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(TreeViewSampleItemContinent item)
		{
			TextAdapter.text = item.Name + " (Countries: " + item.Countries + ") ";
		}

		/// <summary>
		/// Is colors setted at least once?
		/// </summary>
		protected bool GraphicsColorSetted;

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public virtual void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration)
		{
			// reset default color to white, otherwise it will look darker than specified color,
			// because actual color = Text.color * Text.CrossFadeColor
			if (!GraphicsColorSetted)
			{
				TextAdapter.color = Color.white;
			}

			// change color instantly for first time
			var graphic = Utilities.GetGraphic(TextAdapter);
			if (graphic != null)
			{
				graphic.CrossFadeColor(foregroundColor, GraphicsColorSetted ? fadeDuration : 0f, true, true);
			}

			GraphicsColorSetted = true;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Text, ref TextAdapter);
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