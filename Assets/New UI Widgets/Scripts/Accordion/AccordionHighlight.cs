namespace UIWidgets
{
	using UIWidgets.Styles;
	using UnityEngine;

	/// <summary>
	/// Highlight accordion.
	/// </summary>
	[RequireComponent(typeof(Accordion))]
	public class AccordionHighlight : MonoBehaviour, IStylable
	{
		[SerializeField]
		StyleImage defaultToggleBackground;

		/// <summary>
		/// Default background.
		/// </summary>
		public StyleImage DefaultToggleBackground
		{
			get
			{
				return defaultToggleBackground;
			}

			set
			{
				defaultToggleBackground = value;
				UpdateHighlights();
			}
		}

		[SerializeField]
		StyleImage activeToggleBackground;

		/// <summary>
		/// Active background.
		/// </summary>
		public StyleImage ActiveToggleBackground
		{
			get
			{
				return activeToggleBackground;
			}

			set
			{
				activeToggleBackground = value;
				UpdateHighlights();
			}
		}

		Accordion accordion;

		/// <summary>
		/// Accordion.
		/// </summary>
		protected Accordion Accordion
		{
			get
			{
				if (accordion == null)
				{
					accordion = GetComponent<Accordion>();
				}

				return accordion;
			}
		}

		bool isInited;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instances.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

			Accordion.OnStartToggleAnimation.AddListener(OnToggle);
			Accordion.OnDataSourceChanged.AddListener(UpdateHighlights);

			UpdateHighlights();
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (accordion != null)
			{
				accordion.OnStartToggleAnimation.RemoveListener(OnToggle);
				accordion.OnDataSourceChanged.RemoveListener(UpdateHighlights);
			}
		}

		/// <summary>
		/// Process the toggle event.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void OnToggle(AccordionItem item)
		{
			UpdateHighlight(item);
		}

		/// <summary>
		/// Update item highlight.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void UpdateHighlight(AccordionItem item)
		{
			if (item.Open)
			{
				ActiveToggleBackground.ApplyTo(item.ToggleObject);
			}
			else
			{
				DefaultToggleBackground.ApplyTo(item.ToggleObject);
			}
		}

		/// <summary>
		/// Update highlights of all opened items.
		/// </summary>
		public virtual void UpdateHighlights()
		{
			Accordion.DataSource.ForEach(UpdateHighlight);
		}

		/// <inheritdoc/>
		public virtual bool SetStyle(Style style)
		{
			defaultToggleBackground = style.Accordion.ToggleBackground;
			activeToggleBackground = style.Accordion.ToggleActiveBackground;

			UpdateHighlights();

			return true;
		}

		/// <inheritdoc/>
		public virtual bool GetStyle(Style style)
		{
			style.Accordion.ToggleBackground = defaultToggleBackground;
			style.Accordion.ToggleActiveBackground = activeToggleBackground;

			return true;
		}
	}
}