namespace UIWidgets.Examples.Shops
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// Trader list view component.
	/// </summary>
	public class TraderListViewComponent : ListViewItem, IViewData<JRPGOrderLine>
	{
		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with NameAdapter.")]
		public Text Name;

		/// <summary>
		/// The price.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with PriceAdapter.")]
		public Text Price;

		/// <summary>
		/// The available count.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with AvailableCountAdapter.")]
		public Text AvailableCount;

		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		public TextAdapter NameAdapter;

		/// <summary>
		/// The price.
		/// </summary>
		[SerializeField]
		public TextAdapter PriceAdapter;

		/// <summary>
		/// The available count.
		/// </summary>
		[SerializeField]
		public TextAdapter AvailableCountAdapter;

		/// <summary>
		/// Count spinner.
		/// </summary>
		[SerializeField]
		protected Spinner Count;

		/// <summary>
		/// Init graphics foreground.
		/// </summary>
		protected override void GraphicsForegroundInit()
		{
			if (GraphicsForegroundVersion == 0)
			{
				Foreground = new Graphic[]
				{
					Utilities.GetGraphic(NameAdapter),
					Utilities.GetGraphic(PriceAdapter),
					Utilities.GetGraphic(AvailableCountAdapter),
				};
				GraphicsForegroundVersion = 1;
			}
		}

		/// <summary>
		/// OrderLine.
		/// </summary>
		public JRPGOrderLine OrderLine
		{
			get;
			protected set;
		}

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected override void Start()
		{
			Count.onValueChangeInt.AddListener(ChangeCount);
			base.Start();
		}

		/// <summary>
		/// Change count on left and right movements.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnMove(AxisEventData eventData)
		{
			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
					Count.Value -= 1;
					break;
				case MoveDirection.Right:
					Count.Value += 1;
					break;
				default:
					base.OnMove(eventData);
					break;
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="item">Order line.</param>
		public void SetData(JRPGOrderLine item)
		{
			OrderLine = item;

			NameAdapter.text = OrderLine.Item.Name;
			PriceAdapter.text = OrderLine.Price.ToString();
			AvailableCountAdapter.text = (OrderLine.Item.Count == -1) ? "∞" : OrderLine.Item.Count.ToString();

			Count.Min = 0;
			Count.Max = (OrderLine.Item.Count == -1) ? 9999 : OrderLine.Item.Count;
			Count.Value = OrderLine.Count;
		}

		void ChangeCount(int count)
		{
			OrderLine.Count = count;
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected override void OnDestroy()
		{
			if (Count != null)
			{
				Count.onValueChangeInt.RemoveListener(ChangeCount);
			}
		}

		/// <summary>
			/// Upgrade this instance.
			/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Name, ref NameAdapter);
			Utilities.GetOrAddComponent(Price, ref PriceAdapter);
			Utilities.GetOrAddComponent(AvailableCount, ref AvailableCountAdapter);
#pragma warning restore 0612, 0618
		}
	}
}