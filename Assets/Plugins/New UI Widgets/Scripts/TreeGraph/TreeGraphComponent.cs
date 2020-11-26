namespace UIWidgets
{
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TreeGraph component.
	/// </summary>
	/// <typeparam name="T">Node type.</typeparam>
	[RequireComponent(typeof(MultipleConnector))]
	public abstract class TreeGraphComponent<T> : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Gets foreground graphics for coloring.
		/// </summary>
		public virtual Graphic[] GraphicsForeground
		{
			get
			{
				return new Graphic[] { };
			}
		}

		/// <summary>
		/// Background graphics for coloring.
		/// </summary>
		public virtual Graphic[] GraphicsBackground
		{
			get
			{
				return new Graphic[] { Background, };
			}
		}

		/// <summary>
		/// Background.
		/// </summary>
		[SerializeField]
		public Image Background;

		/// <summary>
		/// Node.
		/// </summary>
		protected TreeNode<T> Node;

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
		public virtual void Init()
		{
			if (Background != null)
			{
				return;
			}

			var bg = transform.Find("Background");
			if (bg == null)
			{
				return;
			}

			Background = bg.GetComponent<Image>();
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="node">Node.</param>
		public abstract void SetData(TreeNode<T> node);

		/// <summary>
		/// Called when item moved to cache, you can use it free used resources.
		/// </summary>
		public virtual void MovedToCache()
		{
		}

		/// <summary>
		/// Toggle node visibility.
		/// </summary>
		public virtual void ToggleVisibility()
		{
			Node.IsVisible = !Node.IsVisible;
		}

		/// <summary>
		/// Toggle expanded.
		/// </summary>
		public virtual void ToggleExpanded()
		{
			Node.IsExpanded = !Node.IsExpanded;
		}

		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <param name="styleBackground">Style for the background.</param>
		/// <param name="styleText">Style for the text.</param>
		/// <param name="style">Full style data.</param>
		public virtual void SetStyle(StyleImage styleBackground, StyleText styleText, Style style)
		{
			styleBackground.ApplyTo(Background);

			foreach (var gf in GraphicsForeground)
			{
				if (gf != null)
				{
					styleText.ApplyTo(gf.gameObject);
				}
			}

			var connector = GetComponent<MultipleConnector>();
			connector.color = style.Collections.DefaultColor;
		}

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);

			return true;
		}
		#endregion
	}
}