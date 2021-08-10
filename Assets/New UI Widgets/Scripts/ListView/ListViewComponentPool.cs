namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Extensions;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// ListViewBase.
	/// You can use it for creating custom ListViews.
	/// </summary>
	public abstract partial class ListViewBase : UIBehaviour,
			ISelectHandler, IDeselectHandler,
			ISubmitHandler, ICancelHandler,
			IStylable, IUpgradeable
	{
		/// <summary>
		/// ListView components pool.
		/// </summary>
		/// <typeparam name="TComponent">Type of DefaultItem component.</typeparam>
		/// <typeparam name="TItem">Type of item.</typeparam>
		public class ListViewComponentPool<TComponent, TItem>
			where TComponent : ListViewItem
		{
			/// <summary>
			/// Cache of the Templates instances.
			/// </summary>
			protected Dictionary<int, List<TComponent>> TemplatesCache = new Dictionary<int, List<TComponent>>();

			/// <summary>
			/// Destroy instances of the previous Template when replacing Template.
			/// </summary>
			protected bool destroyComponents = true;

			/// <summary>
			/// Destroy instances of the previous Template when replacing Template.
			/// </summary>
			public bool DestroyComponents
			{
				get
				{
					return destroyComponents;
				}

				set
				{
					destroyComponents = value;

					if (destroyComponents)
					{
						foreach (var components in TemplatesCache.Values)
						{
							components.ForEach(DestroyComponent);
							components.Clear();
						}

						TemplatesCache.Clear();
					}
				}
			}

			/// <summary>
			/// The components gameobjects container.
			/// </summary>
			public Transform Container;

			/// <summary>
			/// The owner.
			/// </summary>
			public ListViewBase Owner;

			/// <summary>
			/// The template.
			/// </summary>
			protected TComponent template;

			/// <summary>
			/// The template.
			/// </summary>
			public TComponent Template
			{
				get
				{
					return template;
				}

				set
				{
					SetTemplate(value);
				}
			}

			/// <summary>
			/// The components list.
			/// </summary>
			protected List<TComponent> Components;

			/// <summary>
			/// The components cache list.
			/// </summary>
			protected List<TComponent> ComponentsCache;

			/// <summary>
			/// The function to add callbacks.
			/// </summary>
			public Action<ListViewItem> CallbackAdd;

			/// <summary>
			/// The function to remove callbacks.
			/// </summary>
			public Action<ListViewItem> CallbackRemove;

			/// <summary>
			/// The displayed indices.
			/// </summary>
			protected List<int> DisplayedIndices;

			/// <summary>
			/// Indices of the added items.
			/// </summary>
			protected List<int> IndicesAdded = new List<int>();

			/// <summary>
			/// Indices of the removed items.
			/// </summary>
			protected List<int> IndicesRemoved = new List<int>();

			/// <summary>
			/// Indices of the removed dragged items.
			/// </summary>
			protected List<int> IndicesDraggedRemoved = new List<int>();

			/// <summary>
			/// Indices of the dragged items.
			/// </summary>
			protected List<int> IndicesDragged = new List<int>();

			/// <summary>
			/// Indices of the untouched items.
			/// </summary>
			protected List<int> IndicesUntouched = new List<int>();

			/// <summary>
			/// Initializes a new instance of the <see cref="ListViewComponentPool{TComponent, TItem}"/> class.
			/// Use parents lists to avoid problem with creating copies of the original ListView.
			/// </summary>
			/// <param name="components">Components list to use.</param>
			/// <param name="componentsCache">Components cache to use.</param>
			/// <param name="displayedIndices">Displayed indices to use.</param>
			public ListViewComponentPool(List<TComponent> components, List<TComponent> componentsCache, List<int> displayedIndices)
			{
				Components = components;
				ComponentsCache = componentsCache;
				DisplayedIndices = displayedIndices;
			}

			/// <summary>
			/// Process locale changes.
			/// </summary>
			public virtual void LocaleChanged()
			{
				for (int i = 0; i < Components.Count; i++)
				{
					Components[i].LocaleChanged();
				}
			}

			/// <summary>
			/// Find component with the specified index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <returns>Component with the specified index.</returns>
			public TComponent Find(int index)
			{
				for (int i = 0; i < Components.Count; i++)
				{
					if (Components[i].Index == index)
					{
						return Components[i];
					}
				}

				return null;
			}

			/// <summary>
			/// Set the DisplayedIndices.
			/// </summary>
			/// <param name="newIndices">New indices.</param>
			/// <param name="action">Action.</param>
			public void DisplayedIndicesSet(List<int> newIndices, Action<TComponent> action)
			{
				SetCount(newIndices.Count);

				for (int i = 0; i < Components.Count; i++)
				{
					Components[i].Index = newIndices[i];
					action(Components[i]);
				}

				DisplayedIndices.Clear();
				DisplayedIndices.AddRange(newIndices);

				Components.Sort(ComponentsComparer);
				Components.ForEach(SetAsLastSibling);

				LayoutRebuilder.ForceRebuildLayoutImmediate(Owner.Container as RectTransform);
			}

			/// <summary>
			/// Check if indices are equal.
			/// </summary>
			/// <param name="newIndices">New indices.</param>
			/// <returns>true if indices are equal; otherwise false.</returns>
			protected bool IndicesEqual(List<int> newIndices)
			{
				if (DisplayedIndices.Count != newIndices.Count)
				{
					return false;
				}

				for (int i = 0; i < DisplayedIndices.Count; i++)
				{
					if (DisplayedIndices[i] != newIndices[i])
					{
						return false;
					}
				}

				return true;
			}

			/// <summary>
			/// Find difference between indices.
			/// </summary>
			/// <param name="newIndices">New indices.</param>
			protected void FindIndicesDiff(List<int> newIndices)
			{
				IndicesAdded.Clear();
				IndicesRemoved.Clear();
				IndicesDraggedRemoved.Clear();
				IndicesDragged.Clear();
				IndicesUntouched.Clear();

				foreach (var component in Components)
				{
					if (component.IsDragged)
					{
						IndicesDragged.Add(component.Index);
					}
				}

				foreach (var new_index in newIndices)
				{
					if (!DisplayedIndices.Contains(new_index))
					{
						IndicesAdded.Add(new_index);
					}
				}

				foreach (var index in DisplayedIndices)
				{
					if (!newIndices.Contains(index))
					{
						if (IndicesDragged.Contains(index))
						{
							IndicesDraggedRemoved.Add(index);
						}
						else
						{
							IndicesRemoved.Add(index);
						}
					}
					else if (!IndicesDragged.Contains(index))
					{
						IndicesUntouched.Add(index);
					}
				}
			}

			/// <summary>
			/// Update the DisplayedIndices.
			/// </summary>
			/// <param name="newIndices">New indices.</param>
			/// <param name="action">Action.</param>
			public void DisplayedIndicesUpdate(List<int> newIndices, Action<TComponent> action)
			{
				if (IndicesEqual(newIndices))
				{
					return;
				}

				FindIndicesDiff(newIndices);

				var added = IndicesAdded.Count;
				for (int i = added; i < IndicesDraggedRemoved.Count; i++)
				{
					var index = IndicesUntouched.Pop();
					IndicesAdded.Add(index);
					IndicesRemoved.Add(index);
				}

				if (IndicesRemoved.Count > 0)
				{
					for (int i = Components.Count - 1; i >= 0; i--)
					{
						var component = Components[i];
						if (IndicesRemoved.Contains(component.Index))
						{
							DeactivateComponent(component);
							Components.RemoveAt(i);
							ComponentsCache.Add(component);
						}
					}
				}

				var removed = 0;
				if (IndicesDraggedRemoved.Count > 0)
				{
					for (int i = Components.Count - 1; i >= 0; i--)
					{
						if (removed == IndicesAdded.Count)
						{
							break;
						}

						var component = Components[i];
						if (IndicesDraggedRemoved.Contains(component.Index))
						{
							Components.RemoveAt(i);
							Components.Add(component);
							removed += 1;
						}
					}
				}

				for (int i = 0; i < (IndicesAdded.Count - removed); i++)
				{
					var component = CreateComponent();
					Components.Add(component);
				}

				SetOwnerItems();

				var start = Components.Count - IndicesAdded.Count;
				for (int i = 0; i < IndicesAdded.Count; i++)
				{
					var component = Components[start + i];
					component.Index = IndicesAdded[i];
					action(component);
				}

				DisplayedIndices.Clear();
				DisplayedIndices.AddRange(newIndices);

				Components.Sort(ComponentsComparer);
				Components.ForEach(SetAsLastSibling);

				LayoutRebuilder.ForceRebuildLayoutImmediate(Owner.Container as RectTransform);
			}

			/// <summary>
			/// Sets the required components count.
			/// </summary>
			/// <param name="count">Count.</param>
			public void SetCount(int count)
			{
				Components.RemoveAll(IsNullComponent);

				if (Components.Count == count)
				{
					return;
				}

				if (Components.Count < count)
				{
					ComponentsCache.RemoveAll(IsNullComponent);

					for (int i = Components.Count; i < count; i++)
					{
						Components.Add(CreateComponent());
					}
				}
				else
				{
					// try to disable components except dragged one
					var index = Components.Count - 1;
					while ((Components.Count > count) && (index >= 0))
					{
						var component = Components[index];
						if (!component.IsDragged)
						{
							DeactivateComponent(component);
							ComponentsCache.Add(component);
							Components.RemoveAt(index);
						}

						index--;
					}

					// if too much dragged components then disable any components
					index = Components.Count - 1;
					while ((Components.Count > count) && (index >= 0))
					{
						var component = Components[index];
						DeactivateComponent(component);
						ComponentsCache.Add(component);
						Components.RemoveAt(index);

						index--;
					}
				}

				SetOwnerItems();
			}

			/// <summary>
			/// Set the owner items.
			/// </summary>
			protected void SetOwnerItems()
			{
				Owner.UpdateComponents<TComponent>(Components);
			}

			/// <summary>
			/// Sets the template.
			/// </summary>
			/// <param name="newTemplate">New template.</param>
			protected virtual void SetTemplate(TComponent newTemplate)
			{
				// clear previous DefaultItem data
				if (template != null)
				{
					template.gameObject.SetActive(false);

					if ((newTemplate != null) && (template.GetInstanceID() == newTemplate.GetInstanceID()))
					{
						return;
					}
				}

				Components.ForEach(DeactivateComponent);

				if (DestroyComponents)
				{
					Components.ForEach(DestroyComponent);
					ComponentsCache.ForEach(DestroyComponent);
				}
				else if (template != null)
				{
					List<TComponent> cache;
					if (!TemplatesCache.TryGetValue(template.GetInstanceID(), out cache))
					{
						cache = new List<TComponent>(Components.Count + ComponentsCache.Count);
						TemplatesCache[template.GetInstanceID()] = cache;
					}

					cache.AddRange(ComponentsCache);
					cache.AddRange(Components);
				}

				ComponentsCache.Clear();
				Components.Clear();

				// set new DefaultItem data
				template = newTemplate;
				if (template != null)
				{
					template.Owner = Owner;
					template.gameObject.SetActive(false);

					if (!DestroyComponents)
					{
						List<TComponent> cached;
						if (TemplatesCache.TryGetValue(template.GetInstanceID(), out cached))
						{
							ComponentsCache.AddRange(cached);
							cached.Clear();
						}
					}
				}
			}

			/// <summary>
			/// Is component is null?
			/// </summary>
			/// <param name="component">Component.</param>
			/// <returns>true if component is null; otherwise, false.</returns>
			protected bool IsNullComponent(TComponent component)
			{
				return component == null;
			}

			/// <summary>
			/// Create component instance.
			/// </summary>
			/// <returns>Component instance.</returns>
			protected virtual TComponent CreateComponent()
			{
				TComponent component;
				if (ComponentsCache.Count > 0)
				{
					component = ComponentsCache[ComponentsCache.Count - 1];
					ComponentsCache.RemoveAt(ComponentsCache.Count - 1);
				}
				else
				{
					component = Compatibility.Instantiate(template, Container);
					Utilities.FixInstantiated(template, component);
					component.Owner = Owner;
				}

				component.Index = -2;
				component.transform.SetAsLastSibling();
				component.gameObject.SetActive(true);

				CallbackAdd(component);

				return component;
			}

			/// <summary>
			/// Deactivates the component.
			/// </summary>
			/// <param name="component">Component.</param>
			protected void DeactivateComponent(TComponent component)
			{
				if (component != null)
				{
					CallbackRemove(component);
					component.MovedToCache();
					component.Index = -1;
					component.gameObject.SetActive(false);
				}
			}

			/// <summary>
			/// Destroy the component.
			/// </summary>
			/// <param name="component">Component.</param>
			protected void DestroyComponent(TComponent component)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(component.gameObject);
				}
#if UNITY_EDITOR
				else
				{
					UnityEngine.Object.DestroyImmediate(component.gameObject);
				}
#endif
			}

			/// <summary>
			/// Compare components by component index.
			/// </summary>
			/// <returns>A signed integer that indicates the relative values of x and y.</returns>
			/// <param name="x">The x coordinate.</param>
			/// <param name="y">The y coordinate.</param>
			protected int ComponentsComparer(TComponent x, TComponent y)
			{
				return DisplayedIndices.IndexOf(x.Index).CompareTo(DisplayedIndices.IndexOf(y.Index));
			}

			/// <summary>
			/// Move the component transform to the end of the local transform list.
			/// </summary>
			/// <param name="item">Item.</param>
			protected void SetAsLastSibling(Component item)
			{
				item.transform.SetAsLastSibling();
			}

			/// <summary>
			/// Apply function for each component.
			/// </summary>
			/// <param name="action">Action.</param>
			public void ForEach(Action<TComponent> action)
			{
				Components.ForEach(action);
			}

			/// <summary>
			/// Apply function for each component and cached components.
			/// </summary>
			/// <param name="action">Action.</param>
			public void ForEachAll(Action<TComponent> action)
			{
				action(Template);
				Components.ForEach(action);
				ComponentsCache.ForEach(action);
			}

			/// <summary>
			/// Apply function for each cached component.
			/// </summary>
			/// <param name="action">Action.</param>
			public void ForEachCache(Action<TComponent> action)
			{
				ComponentsCache.ForEach(action);
			}

			/// <summary>
			/// Apply function for each cached component.
			/// </summary>
			/// <param name="action">Action.</param>
			public void ForEachCache(Action<ListViewItem> action)
			{
				for (int i = 0; i < ComponentsCache.Count; i++)
				{
					action(ComponentsCache[i]);
				}
			}

			/// <summary>
			/// Get the copy of components list.
			/// </summary>
			/// <returns>Components list.</returns>
			public List<TComponent> List()
			{
				return new List<TComponent>(Components);
			}

			/// <summary>
			/// Set size of the components.
			/// </summary>
			/// <param name="size">Size.</param>
			public void SetSize(Vector2 size)
			{
				SetSize(Template, size);

				for (int i = 0; i < Components.Count; i++)
				{
					SetSize(Components[i], size);
				}

				for (int i = 0; i < ComponentsCache.Count; i++)
				{
					SetSize(ComponentsCache[i], size);
				}
			}

			/// <summary>
			/// Set size.
			/// </summary>
			/// <param name="component">Component.</param>
			/// <param name="size">Size.</param>
			protected void SetSize(TComponent component, Vector2 size)
			{
				var item_rt = component.transform as RectTransform;
				item_rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
				item_rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
			}

			/// <summary>
			/// Set the style.
			/// </summary>
			/// <param name="styleBackground">Style for the background.</param>
			/// <param name="styleText">Style for the text.</param>
			/// <param name="style">Full style data.</param>
			public void SetStyle(StyleImage styleBackground, StyleText styleText, Style style)
			{
				Template.SetStyle(styleBackground, styleText, style);

				for (int i = 0; i < Components.Count; i++)
				{
					Components[i].SetStyle(styleBackground, styleText, style);
				}

				for (int i = 0; i < ComponentsCache.Count; i++)
				{
					ComponentsCache[i].SetStyle(styleBackground, styleText, style);
				}
			}
		}
	}
}