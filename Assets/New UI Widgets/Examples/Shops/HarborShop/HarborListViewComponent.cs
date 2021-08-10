namespace UIWidgets.Examples.Shops
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// HarborListViewComponent.
	/// </summary>
	public class HarborListViewComponent : ListViewItem, IViewData<HarborOrderLine>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with NameAdapter.")]
		public Text Name;

		/// <summary>
		/// Sell price.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with SellPriceAdapter.")]
		public Text SellPrice;

		/// <summary>
		/// Buy price.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with BuyPriceAdapter.")]
		public Text BuyPrice;

		/// <summary>
		/// Available buy count.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with AvailableBuyCountAdapter.")]
		public Text AvailableBuyCount;

		/// <summary>
		/// Available sell count.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[System.Obsolete("Replaced with AvailableSellCountAdapter.")]
		public Text AvailableSellCount;

		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public TextAdapter NameAdapter;

		/// <summary>
		/// Sell price.
		/// </summary>
		[SerializeField]
		public TextAdapter SellPriceAdapter;

		/// <summary>
		/// Buy price.
		/// </summary>
		[SerializeField]
		public TextAdapter BuyPriceAdapter;

		/// <summary>
		/// Available buy count.
		/// </summary>
		[SerializeField]
		public TextAdapter AvailableBuyCountAdapter;

		/// <summary>
		/// Available sell count.
		/// </summary>
		[SerializeField]
		public TextAdapter AvailableSellCountAdapter;

		/// <summary>
		/// Count slider.
		/// </summary>
		[SerializeField]
		protected CenteredSlider Count;

		/// <summary>
		/// Current order line.
		/// </summary>
		public HarborOrderLine OrderLine;

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
					Utilities.GetGraphic(BuyPriceAdapter),
					Utilities.GetGraphic(SellPriceAdapter),
					Utilities.GetGraphic(AvailableBuyCountAdapter),
					Utilities.GetGraphic(AvailableSellCountAdapter),
				};
				GraphicsForegroundVersion = 1;
			}
		}

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected override void Start()
		{
			Count.OnValueChanged.AddListener(ChangeCount);
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
		public void SetData(HarborOrderLine item)
		{
			OrderLine = item;

			NameAdapter.text = OrderLine.Item.Name;

			BuyPriceAdapter.text = OrderLine.BuyPrice.ToString();
			SellPriceAdapter.text = OrderLine.SellPrice.ToString();

			AvailableBuyCountAdapter.text = OrderLine.BuyCount.ToString();
			AvailableSellCountAdapter.text = OrderLine.SellCount.ToString();

			Count.LimitMin = -OrderLine.SellCount;
			Count.LimitMax = OrderLine.BuyCount;
			Count.Value = OrderLine.Count;
		}

		void ChangeCount(int value)
		{
			OrderLine.Count = value;
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected override void OnDestroy()
		{
			if (Count != null)
			{
				Count.OnValueChanged.RemoveListener(ChangeCount);
			}
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
#pragma warning disable 0612, 0618
			Utilities.GetOrAddComponent(Name, ref NameAdapter);
			Utilities.GetOrAddComponent(BuyPrice, ref BuyPriceAdapter);
			Utilities.GetOrAddComponent(SellPrice, ref SellPriceAdapter);
			Utilities.GetOrAddComponent(AvailableBuyCount, ref AvailableBuyCountAdapter);
			Utilities.GetOrAddComponent(AvailableSellCount, ref AvailableSellCountAdapter);
#pragma warning restore 0612, 0618
		}
	}
}