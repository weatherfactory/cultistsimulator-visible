namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// GroupedListViewComponent.
	/// </summary>
	public class GroupedListViewComponent : ListViewItem, IViewData<IGroupedListItem>
	{
		/// <summary>
		/// Item group template.
		/// </summary>
		[SerializeField]
		protected GroupedListViewComponentGroup GroupTemplate;

		/// <summary>
		/// Item template.
		/// </summary>
		[SerializeField]
		protected GroupedListViewComponentItem ItemTemplate;

		/// <summary>
		/// Items parent.
		/// </summary>
		[SerializeField]
		protected Transform ComponentParent;

		IGroupedListViewComponent CurrentComponent;

		/// <summary>
		/// Init templates.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			GroupTemplate.gameObject.SetActive(false);
			ItemTemplate.gameObject.SetActive(false);
		}

		/// <summary>
		/// Gets the template.
		/// </summary>
		/// <returns>The template.</returns>
		/// <param name="type">Type.</param>
		protected virtual IGroupedListViewComponent GetTemplate(Type type)
		{
			if (type == typeof(GroupedListGroup))
			{
				return GroupTemplate;
			}

			if (type == typeof(GroupedListItem))
			{
				return ItemTemplate;
			}

			return null;
		}

		/// <summary>
		/// Item.
		/// </summary>
		protected IGroupedListItem Item;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(IGroupedListItem item)
		{
			// move current component to cache
			MovedToCache();

			Item = item;

			// get template by type (or by any other item value)
			var template = GetTemplate(item.GetType());

			// instantiate template copy, ComponentParent can be DefaultItem itself or one of it children gameobjects
			CurrentComponent = template.IInstance(ComponentParent);

			// set data to display item
			CurrentComponent.SetData(item);
		}

		/// <summary>
		/// Is graphics colors setted?
		/// </summary>
		protected bool IsColorSetted;

		/// <summary>
		/// Set graphics colors.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public override void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration = 0f)
		{
			base.GraphicsColoring(foregroundColor, backgroundColor, fadeDuration);

			if (CurrentComponent != null)
			{
				CurrentComponent.GraphicsColoring(foregroundColor, backgroundColor, fadeDuration);
			}
		}

		/// <summary>
		/// Free current component.
		/// </summary>
		public override void MovedToCache()
		{
			if ((CurrentComponent != null) && (Item != null))
			{
				var template = GetTemplate(Item.GetType());
				if (template != null)
				{
					var parent = (template as MonoBehaviour).transform.parent;

					CurrentComponent.Free(parent);
					CurrentComponent = null;
				}
			}
		}
	}
}