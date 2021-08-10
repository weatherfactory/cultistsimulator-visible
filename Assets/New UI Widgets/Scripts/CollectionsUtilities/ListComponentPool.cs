namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Extensions;
	using UnityEngine;

	/// <summary>
	/// List component pool.
	/// </summary>
	/// <typeparam name="TComponent">Component type.</typeparam>
	public class ListComponentPool<TComponent>
		where TComponent : Component
	{
		readonly List<TComponent> active;

		readonly List<TComponent> cache;

		readonly RectTransform parent;

		readonly bool MovableToCache;

		TComponent defaultItem;

		/// <summary>
		/// Default item.
		/// </summary>
		public TComponent DefaultItem
		{
			get
			{
				return defaultItem;
			}

			set
			{
				if (defaultItem != value)
				{
					defaultItem = value;

					Require(0);

					cache.ForEach(UnityEngine.Object.Destroy);
					cache.Clear();

					if (defaultItem != null)
					{
						defaultItem.gameObject.SetActive(false);
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListComponentPool{TComponent}"/> class.
		/// </summary>
		/// <param name="defaultItem">Default item.</param>
		/// <param name="active">List of the active items.</param>
		/// <param name="cache">List of the cached items.</param>
		/// <param name="parent">Parent.</param>
		public ListComponentPool(TComponent defaultItem, List<TComponent> active, List<TComponent> cache, RectTransform parent)
		{
			MovableToCache = typeof(IMovableToCache).IsAssignableFrom(typeof(TComponent));

			this.defaultItem = defaultItem;
			if (this.defaultItem != null)
			{
				this.defaultItem.gameObject.SetActive(false);
			}

			this.active = active;
			this.cache = cache;
			this.parent = parent;
		}

		/// <summary>
		/// Get component instance by the specified index.
		/// </summary>
		/// <param name="index">Item.</param>
		/// <returns>Component instance.</returns>
		public TComponent this[int index]
		{
			get
			{
				return active[index];
			}
		}

		/// <summary>
		/// Count of the active components.
		/// </summary>
		public int Count
		{
			get
			{
				return active.Count;
			}
		}

		/// <summary>
		/// Set the count of the active instances.
		/// </summary>
		/// <param name="instancesCount">Required instances count.</param>
		public void Require(int instancesCount)
		{
			if (active.Count > instancesCount)
			{
				for (var i = active.Count - 1; i >= instancesCount; i--)
				{
					var instance = active[i];

					if (MovableToCache)
					{
						(instance as IMovableToCache).MovedToCache();
					}

					instance.gameObject.SetActive(false);
					cache.Add(instance);

					active.RemoveAt(i);
				}
			}

			while (active.Count < instancesCount)
			{
				GetInstance();
			}
		}

		/// <summary>
		/// Disable components by indices.
		/// </summary>
		/// <param name="indices">Indices of the components to disable.</param>
		public void Disable(List<int> indices)
		{
			indices.Sort();

			for (int i = indices.Count - 1; i >= 0; i--)
			{
				var index = indices[i];
				var instance = active[index];

				if (MovableToCache)
				{
					(instance as IMovableToCache).MovedToCache();
				}

				instance.gameObject.SetActive(false);
				cache.Add(instance);

				active.RemoveAt(index);
			}
		}

		/// <summary>
		/// Get instance.
		/// </summary>
		/// <returns>Instance.</returns>
		public TComponent GetInstance()
		{
			TComponent instance;

			if (cache.Count > 0)
			{
				instance = cache.Pop();
			}
			else
			{
				instance = Compatibility.Instantiate(DefaultItem);
				Utilities.FixInstantiated(DefaultItem, instance);
				instance.transform.SetParent(parent, false);
			}

			instance.gameObject.SetActive(true);
			active.Add(instance);

			return instance;
		}

		/// <summary>
		/// Apply function for each component.
		/// </summary>
		/// <param name="action">Action.</param>
		public void ForEach(Action<TComponent> action)
		{
			active.ForEach(action);
		}

		/// <summary>
		/// Apply function for each component and cached components.
		/// </summary>
		/// <param name="action">Action.</param>
		public void ForEachAll(Action<TComponent> action)
		{
			active.ForEach(action);
			cache.ForEach(action);
		}

		/// <summary>
		/// Apply function for each cached component.
		/// </summary>
		/// <param name="action">Action.</param>
		public void ForEachCache(Action<TComponent> action)
		{
			cache.ForEach(action);
		}
	}
}