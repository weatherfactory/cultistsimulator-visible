namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TableList component.
	/// </summary>
	public class TableListComponent : ListViewItem, IViewData<List<int>>
	{
		/// <summary>
		/// The text components.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with TextAdapterComponents.")]
		public List<Text> TextComponents = new List<Text>();

		/// <summary>
		/// The text components.
		/// </summary>
		[SerializeField]
		public List<TextAdapter> TextAdapterComponents = new List<TextAdapter>();

		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsForeground
		{
			get
			{
				var result = new Graphic[TextAdapterComponents.Count];

				for (int i = 0; i < TextAdapterComponents.Count; i++)
				{
					result[i] = Utilities.GetGraphic(TextAdapterComponents[i]);
				}

				return result;
			}
		}

		/// <summary>
		/// Background graphics for coloring.
		/// </summary>
		public override Graphic[] GraphicsBackground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Gets the objects to resize.
		/// </summary>
		/// <value>The objects to resize.</value>
		public GameObject[] ObjectsToResize
		{
			get
			{
				var result = new GameObject[TextAdapterComponents.Count];

				for (int i = 0; i < TextAdapterComponents.Count; i++)
				{
					result[i] = TextAdapterComponents[i].transform.parent.gameObject;
				}

				return result;
			}
		}

		/// <summary>
		/// The item.
		/// </summary>
		[SerializeField]
		protected List<int> Item;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(List<int> item)
		{
			Item = item;
			UpdateView();
		}

		/// <summary>
		/// Update text components text.
		/// </summary>
		public void UpdateView()
		{
			TextAdapterComponents.ForEach(SetData);
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="index">Index.</param>
		protected void SetData(TextAdapter text, int index)
		{
			text.text = Item.Count > index ? Item[index].ToString() : "none";
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			if (TextAdapterComponents.Count == 0)
			{
				foreach (var t in TextComponents)
				{
					TextAdapterComponents.Add(Utilities.GetOrAddComponent<TextAdapter>(t));
				}
			}
#pragma warning restore 0612, 0618
		}
	}
}