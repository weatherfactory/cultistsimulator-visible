namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using UIWidgets.Attributes;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TreeGraph directions.
	/// </summary>
	public enum TreeGraphDirections
	{
		/// <summary>
		/// Top to bottom.
		/// </summary>
		TopToBottom = 0,

		/// <summary>
		/// Bottom to top
		/// </summary>
		BottomToTop = 1,

		/// <summary>
		/// Left to right.
		/// </summary>
		LeftToRight = 2,

		/// <summary>
		/// Right to left.
		/// </summary>
		RightToLeft = 3,
	}

	/// <summary>
	/// Base class for TreeGraph's.
	/// </summary>
	/// <typeparam name="TItem">Item type.</typeparam>
	/// <typeparam name="TComponent">Component type.</typeparam>
	[DataBindSupport]
	public class TreeGraphCustom<TItem, TComponent> : MonoBehaviour, IStylable
		where TComponent : TreeGraphComponent<TItem>
	{
		[SerializeField]
		ObservableList<TreeNode<TItem>> nodes = new ObservableList<TreeNode<TItem>>();

		/// <summary>
		/// Gets or sets the nodes.
		/// </summary>
		/// <value>The nodes.</value>
		[DataBindField]
		public virtual ObservableList<TreeNode<TItem>> Nodes
		{
			get
			{
				return nodes;
			}

			set
			{
				if (nodes != null)
				{
					nodes.OnChange -= NodesChanged;

					new TreeNode<TItem>(default(TItem))
					{
						Nodes = nodes,
					};
				}

				nodes = value;
				Refresh();
				if (nodes != null)
				{
					nodes.OnChange += NodesChanged;
				}
			}
		}

		[SerializeField]
		TreeGraphDirections direction;

		/// <summary>
		/// Direction.
		/// </summary>
		public TreeGraphDirections Direction
		{
			get
			{
				return direction;
			}

			set
			{
				if (direction != value)
				{
					direction = value;

					Refresh();
				}
			}
		}

		[SerializeField]
		TComponent defaultItem;

		/// <summary>
		/// Default item component.
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

					if (isInited)
					{
						defaultItem.gameObject.SetActive(false);

						var rt = defaultItem.transform as RectTransform;
						rt.anchorMin = Vector2.zero;
						rt.anchorMax = Vector2.zero;
						rt.pivot = new Vector2(0.5f, 0.5f);

						ResetComponents();
						Cache.ForEach(DestroyGameObject);
						ComponentSize = GetComponentSize(value);

						Refresh();
					}
				}
			}
		}

		[SerializeField]
		RectTransform container;

		/// <summary>
		/// Container.
		/// </summary>
		public RectTransform Container
		{
			get
			{
				return container;
			}

			set
			{
				if (container != value)
				{
					ChangeContainer(value);

					ResetComponents();
					Cache.ForEach(DestroyGameObject);

					Refresh();
				}
			}
		}

		/// <summary>
		/// Container LayoutElement.
		/// </summary>
		protected LayoutElement ContainerLayoutElement;

		[SerializeField]
		[Tooltip("Empty space between nodes.")]
		Vector2 spacing;

		/// <summary>
		/// Spacing.
		/// </summary>
		public Vector2 Spacing
		{
			get
			{
				return spacing;
			}

			set
			{
				if (spacing != value)
				{
					spacing = value;

					Refresh();
				}
			}
		}

		/// <summary>
		/// Component size.
		/// </summary>
		protected Vector2 ComponentSize = Vector2.zero;

		/// <summary>
		/// Component cache.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> Cache = new List<TComponent>();

		/// <summary>
		/// Currently used components.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> Used = new List<TComponent>();

		bool isInited;

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

			ChangeContainer(container);

			var rt = DefaultItem.transform as RectTransform;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.zero;
			rt.pivot = new Vector2(0.5f, 0.5f);

			DefaultItem.gameObject.SetActive(false);

			ComponentSize = GetComponentSize(defaultItem);

			Refresh();
		}

		/// <summary>
		/// Change container.
		/// </summary>
		/// <param name="value">New container.</param>
		protected virtual void ChangeContainer(RectTransform value)
		{
			if (value == null)
			{
				value = transform as RectTransform;
			}

			if (container != null)
			{
				var resize_listener = container.GetComponent<ResizeListener>();
				if (resize_listener != null)
				{
					resize_listener.OnResize.RemoveListener(SizeChanged);
				}
			}

			container = value;

			if (container != null)
			{
				var resize_listener = Utilities.GetOrAddComponent<ResizeListener>(container);
				resize_listener.OnResize.AddListener(SizeChanged);

				ContainerLayoutElement = Utilities.GetOrAddComponent<LayoutElement>(container);
			}
		}

		/// <summary>
		/// Get component size.
		/// </summary>
		/// <param name="defaultItemComponent">Default item component.</param>
		/// <returns>Component size.</returns>
		protected virtual Vector2 GetComponentSize(TComponent defaultItemComponent)
		{
			LayoutUtilities.UpdateLayoutsRecursive(defaultItemComponent);

			return (defaultItemComponent.transform as RectTransform).rect.size;
		}

		/// <summary>
		/// Is container size changed?
		/// </summary>
		protected bool ContainerSizeChanged;

		/// <summary>
		/// Handle container size changed event?
		/// </summary>
		protected virtual void SizeChanged()
		{
			ContainerSizeChanged = true;
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			if (ContainerSizeChanged)
			{
				ContainerSizeChanged = false;
				Refresh();
			}
		}

		/// <summary>
		/// Handle nodes changed event.
		/// </summary>
		protected virtual void NodesChanged()
		{
			Refresh();
		}

		/// <summary>
		/// Refresh displayed nodes.
		/// </summary>
		public virtual void Refresh()
		{
			if (!isInited)
			{
				return;
			}

			if (Nodes == null)
			{
				return;
			}

			ResetComponents();

			var size = GetNodesSize(Nodes);
			ContainerLayoutElement.minWidth = size.x;
			ContainerLayoutElement.minHeight = size.y;

			DisplayNodes(Nodes);
		}

		/// <summary>
		/// Display nodes.
		/// </summary>
		/// <param name="nodesToDisplay">Nodes to display.</param>
		protected virtual void DisplayNodes(ObservableList<TreeNode<TItem>> nodesToDisplay)
		{
			if (nodesToDisplay == null)
			{
				return;
			}

			var position = GetStartPosition();
			foreach (var node in nodesToDisplay)
			{
				if (!node.IsVisible)
				{
					continue;
				}

				var size = DisplayNode(node, null, position);
				position = GetNextPosition(position, size);
			}
		}

		/// <summary>
		/// Get start position.
		/// </summary>
		/// <returns>Start position.</returns>
		protected virtual Vector2 GetStartPosition()
		{
			var size = Container.rect.size;
			switch (Direction)
			{
				case TreeGraphDirections.TopToBottom:
					return new Vector2(0, -size.y + (ComponentSize.y / 2f));
				case TreeGraphDirections.BottomToTop:
					return new Vector2(0, -ComponentSize.y / 2f);
				case TreeGraphDirections.LeftToRight:
					return new Vector2(ComponentSize.x / 2f, -size.y);
				case TreeGraphDirections.RightToLeft:
					return new Vector2(size.x - (ComponentSize.x / 2f), -size.y);
				default:
#if NETFX_CORE
					throw new ArgumentException("Unsupported direction: " + Direction);
#else
					throw new InvalidEnumArgumentException("Unsupported direction: " + Direction);
#endif
			}
		}

		/// <summary>
		/// Get next position.
		/// </summary>
		/// <param name="position">Base position.</param>
		/// <param name="size">Size.</param>
		/// <returns>Nex position.</returns>
		protected virtual Vector2 GetNextPosition(Vector2 position, Vector2 size)
		{
			return IsHorizontal()
				? new Vector2(position.x, position.y + size.y + spacing.y)
				: new Vector2(position.x + size.x + spacing.x, position.y);
		}

		/// <summary>
		/// Get next level position.
		/// </summary>
		/// <param name="position">Base position.</param>
		/// <returns>Next level position.</returns>
		protected virtual Vector2 GetNextLevelPosition(Vector2 position)
		{
			var delta = IsHorizontal() ? ComponentSize.x + spacing.x : ComponentSize.y + spacing.y;
			switch (Direction)
			{
				case TreeGraphDirections.TopToBottom:
					return new Vector2(position.x, position.y + delta);
				case TreeGraphDirections.BottomToTop:
					return new Vector2(position.x, position.y - delta);
				case TreeGraphDirections.LeftToRight:
					return new Vector2(position.x + delta, position.y);
				case TreeGraphDirections.RightToLeft:
					return new Vector2(position.x - delta, position.y);
				default:
#if NETFX_CORE
					throw new ArgumentException("Unsupported direction: " + Direction);
#else
					throw new InvalidEnumArgumentException("Unsupported direction: " + Direction);
#endif
			}
		}

		/// <summary>
		/// Display nodes.
		/// </summary>
		/// <param name="nodesToDisplay">Nodes to display.</param>
		/// <param name="connector">Connector.</param>
		/// <param name="position">Start position.</param>
		protected virtual void DisplayNodes(ObservableList<TreeNode<TItem>> nodesToDisplay, MultipleConnector connector, Vector2 position)
		{
			if (nodesToDisplay == null)
			{
				return;
			}

			foreach (var node in nodesToDisplay)
			{
				if (!node.IsVisible)
				{
					continue;
				}

				var size = DisplayNode(node, connector, position);
				position = GetNextPosition(position, size);
			}
		}

		/// <summary>
		/// Display node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="connector">Connector.</param>
		/// <param name="position">Position.</param>
		/// <returns>Node size.</returns>
		protected virtual Vector2 DisplayNode(TreeNode<TItem> node, MultipleConnector connector, Vector2 position)
		{
			var component = GetComponentInstance();

			var size = GetNodeSize(node);

			(component.transform as RectTransform).anchoredPosition = IsHorizontal()
				? new Vector2(position.x, -position.y - (size.y / 2f))
				: new Vector2(position.x + (size.x / 2f), -position.y);

			component.SetData(node);

			if (connector != null)
			{
				var start = ConnectorPosition.Center;
				var end = ConnectorPosition.Center;

				switch (Direction)
				{
					case TreeGraphDirections.TopToBottom:
						start = ConnectorPosition.Bottom;
						end = ConnectorPosition.Top;
						break;
					case TreeGraphDirections.BottomToTop:
						start = ConnectorPosition.Top;
						end = ConnectorPosition.Bottom;
						break;
					case TreeGraphDirections.LeftToRight:
						start = ConnectorPosition.Right;
						end = ConnectorPosition.Left;
						break;
					case TreeGraphDirections.RightToLeft:
						start = ConnectorPosition.Left;
						end = ConnectorPosition.Right;
						break;
					default:
#if NETFX_CORE
						throw new ArgumentException("Unsupported direction: " + Direction);
#else
						throw new InvalidEnumArgumentException("Unsupported direction: " + Direction);
#endif
				}

				var line = new ConnectorLine()
				{
					Target = component.transform as RectTransform,
					Start = start,
					End = end,
					Thickness = 1f,
					Type = ConnectorType.Straight,
				};
				connector.Lines.Add(line);
			}

			if (node.IsExpanded)
			{
				var new_position = GetNextLevelPosition(position);
				DisplayNodes(node.Nodes, component.GetComponent<MultipleConnector>(), new_position);
			}

			return size;
		}

		/// <summary>
		/// Get new component instance.
		/// </summary>
		/// <returns>Component instance.</returns>
		protected TComponent GetComponentInstance()
		{
			TComponent component;
			if (Cache.Count > 0)
			{
				component = Cache.Pop();
			}
			else
			{
				component = Compatibility.Instantiate(defaultItem);
				component.transform.SetParent(container, false);
				Utilities.FixInstantiated(defaultItem, component);
			}

			component.gameObject.SetActive(true);

			Used.Add(component);

			return component;
		}

		/// <summary>
		/// Reset components.
		/// </summary>
		protected void ResetComponents()
		{
			Used.ForEach(ResetComponent);
		}

		/// <summary>
		/// Reset component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected void ResetComponent(TComponent component)
		{
			component.MovedToCache();
			component.GetComponent<MultipleConnector>().Lines.Clear();
			component.gameObject.SetActive(false);
			Cache.Add(component);
		}

		/// <summary>
		/// Get nodes size.
		/// </summary>
		/// <param name="displayNodes">Nodes.</param>
		/// <returns>Nodes size.</returns>
		protected virtual Vector2 GetNodesSize(ObservableList<TreeNode<TItem>> displayNodes)
		{
			var result = Vector2.zero;
			if (displayNodes == null)
			{
				return result;
			}

			foreach (var node in displayNodes)
			{
				if (!node.IsVisible)
				{
					continue;
				}

				var size = GetNodeSize(node);
				if (IsHorizontal())
				{
					result.y += size.y + spacing.y;
					result.x = Mathf.Max(result.x, size.x);
				}
				else
				{
					result.x += size.x + spacing.x;
					result.y = Mathf.Max(result.y, size.y);
				}
			}

			if (result != Vector2.zero)
			{
				if (IsHorizontal())
				{
					result.y -= spacing.y;
				}
				else
				{
					result.x -= spacing.x;
				}
			}

			return result;
		}

		/// <summary>
		/// Get node size.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <returns>Node size.</returns>
		protected virtual Vector2 GetNodeSize(TreeNode<TItem> node)
		{
			var result = Vector2.zero;
			if (node.IsExpanded && (node.Nodes != null))
			{
				foreach (var subnode in node.Nodes)
				{
					if (!subnode.IsVisible)
					{
						continue;
					}

					var subsize = GetNodeSize(subnode);
					if (IsHorizontal())
					{
						result.y += subsize.y + spacing.y;
						result.x = Mathf.Max(result.x, subsize.x);
					}
					else
					{
						result.x += subsize.x + spacing.x;
						result.y = Mathf.Max(result.y, subsize.y);
					}
				}
			}

			if (result == Vector2.zero)
			{
				return ComponentSize;
			}
			else
			{
				if (IsHorizontal())
				{
					result.y -= spacing.y;
					result.x += ComponentSize.x + spacing.x;
				}
				else
				{
					result.x -= spacing.x;
					result.y += ComponentSize.y + spacing.y;
				}
			}

			return result;
		}

		/// <summary>
		/// Is direction is horizontal?
		/// </summary>
		/// <returns>true if direction is horizontal; otherwise, false.</returns>
		protected bool IsHorizontal()
		{
			return Direction == TreeGraphDirections.LeftToRight || Direction == TreeGraphDirections.RightToLeft;
		}

		/// <summary>
		/// Destroy gameobject of the specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected void DestroyGameObject(TComponent component)
		{
			Destroy(component.gameObject);
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (nodes != null)
			{
				nodes.OnChange -= NodesChanged;
			}
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			if (defaultItem != null)
			{
				defaultItem.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}

			if (Used != null)
			{
				for (int i = 0; i < Used.Count; i++)
				{
					Used[i].SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
				}
			}

			if (Cache != null)
			{
				for (int i = 0; i < Cache.Count; i++)
				{
					Cache[i].SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
				}
			}

			return true;
		}
		#endregion
	}
}