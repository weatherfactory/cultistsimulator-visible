namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// List view with GameObjects.
	/// Outdated. Replaced with ListViewCustom. It's provide better interface and usability.
	/// </summary>
	[Obsolete("Outdated. Replaced with ListViewCustom. It's provide better interface and usability.")]
	public class ListViewGameObjects : ListViewBase
	{
		[SerializeField]
		List<GameObject> objects = new List<GameObject>();

		/// <summary>
		/// Gets the objects.
		/// </summary>
		/// <value>The objects.</value>
		public List<GameObject> Objects
		{
			get
			{
				return new List<GameObject>(objects);
			}

			protected set
			{
				UpdateItems(value);
			}
		}

		/// <summary>
		/// Sort function.
		/// </summary>
		public Func<IEnumerable<GameObject>, IEnumerable<GameObject>> SortFunc = null;

		/// <summary>
		/// What to do when the object selected.
		/// </summary>
		public ListViewGameObjectsEvent OnSelectObject = new ListViewGameObjectsEvent();

		/// <summary>
		/// What to do when the object deselected.
		/// </summary>
		public ListViewGameObjectsEvent OnDeselectObject = new ListViewGameObjectsEvent();

		/// <summary>
		/// What to do when the event system send a pointer enter Event.
		/// </summary>
		public ListViewGameObjectsEvent OnPointerEnterObject = new ListViewGameObjectsEvent();

		/// <summary>
		/// What to do when the event system send a pointer exit Event.
		/// </summary>
		public ListViewGameObjectsEvent OnPointerExitObject = new ListViewGameObjectsEvent();

		[NonSerialized]
		bool isListViewGameObjectsInited = false;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isListViewGameObjectsInited)
			{
				return;
			}

			isListViewGameObjectsInited = true;

			base.Init();

			DestroyGameObjects = true;

			UpdateItems();

			OnSelect.AddListener(OnSelectCallback);
			OnDeselect.AddListener(OnDeselectCallback);
		}

		/// <summary>
		/// Process the select callback event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="item">Item.</param>
		void OnSelectCallback(int index, ListViewItem item)
		{
			OnSelectObject.Invoke(index, objects[index]);
		}

		/// <summary>
		/// Process the deselect callback event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="item">Item.</param>
		void OnDeselectCallback(int index, ListViewItem item)
		{
			OnDeselectObject.Invoke(index, objects[index]);
		}

		/// <summary>
		/// Process the pointer enter callback event.
		/// </summary>
		/// <param name="item">Item.</param>
		void OnPointerEnterCallback(ListViewItem item)
		{
			OnPointerEnterObject.Invoke(item.Index, objects[item.Index]);
		}

		/// <summary>
		/// Process the pointer exit callback event.
		/// </summary>
		/// <param name="item">Item.</param>
		void OnPointerExitCallback(ListViewItem item)
		{
			OnPointerExitObject.Invoke(item.Index, objects[item.Index]);
		}

		/// <summary>
		/// Updates the items.
		/// </summary>
		public override void UpdateItems()
		{
			UpdateItems(objects);
		}

		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of added item.</returns>
		public virtual int Add(GameObject item)
		{
			var newObjects = Objects;
			newObjects.Add(item);
			UpdateItems(newObjects);

			var index = objects.IndexOf(item);

			return index;
		}

		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of removed item.</returns>
		public virtual int Remove(GameObject item)
		{
			var index = objects.IndexOf(item);
			if (index == -1)
			{
				return index;
			}

			var newObjects = Objects;
			newObjects.Remove(item);
			UpdateItems(newObjects);

			return index;
		}

		void RemoveCallback(ListViewItem item)
		{
			if (item == null)
			{
				return;
			}

			item.onPointerEnterItem.RemoveListener(OnPointerEnterCallback);
			item.onPointerExitItem.RemoveListener(OnPointerExitCallback);
		}

		/// <summary>
		/// Removes the callbacks.
		/// </summary>
		void RemoveCallbacks()
		{
			Items.ForEach(RemoveCallback);
		}

		/// <summary>
		/// Adds the callbacks.
		/// </summary>
		void AddCallbacks()
		{
			Items.ForEach(AddCallback);
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		void AddCallback(ListViewItem item)
		{
			item.onPointerEnterItem.AddListener(OnPointerEnterCallback);
			item.onPointerExitItem.AddListener(OnPointerExitCallback);
		}

		/// <summary>
		/// Sorts the items.
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="newItems">New items.</param>
		List<GameObject> SortItems(IEnumerable<GameObject> newItems)
		{
			var temp = newItems;
			if (SortFunc != null)
			{
				temp = SortFunc(temp);
			}

			return new List<GameObject>(temp);
		}

		/// <summary>
		/// Clear items of this instance.
		/// </summary>
		public override void Clear()
		{
			if (DestroyGameObjects)
			{
				objects.ForEach(Destroy);
			}

			UpdateItems(new List<GameObject>());
		}

		/// <summary>
		/// Updates the items.
		/// </summary>
		/// <param name="newItems">New items.</param>
		void UpdateItems(List<GameObject> newItems)
		{
			RemoveCallbacks();

			newItems = SortItems(newItems);

			var new_selected_indices = new List<int>();
			var old_selected_indices = SelectedIndices;

			foreach (var index in old_selected_indices)
			{
				var new_index = objects.Count > index ? newItems.IndexOf(objects[index]) : -1;
				if (new_index != -1)
				{
					new_selected_indices.Add(new_index);
				}
				else
				{
					Deselect(index);
				}
			}

			objects = newItems;
			Items = newItems.Convert<GameObject, ListViewItem>(Utilities.GetOrAddComponent<ListViewItem>);

			SelectedIndices = new_selected_indices;

			AddCallbacks();
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			OnSelect.RemoveListener(OnSelectCallback);
			OnDeselect.RemoveListener(OnDeselectCallback);

			RemoveCallbacks();

			base.OnDestroy();
		}
	}
}