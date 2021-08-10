namespace UIWidgets.Examples.Shops
{
	using System.Collections.Generic;
	using UIWidgets;
	using UIWidgets.Extensions;

	/// <summary>
	/// JRPG order.
	/// </summary>
	public class JRPGOrder : IOrder
	{
		readonly List<JRPGOrderLine> OrderLines = new List<JRPGOrderLine>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.Examples.Shops.JRPGOrder"/> class.
		/// </summary>
		/// <param name="orderLines">Order lines.</param>
		public JRPGOrder(ObservableList<JRPGOrderLine> orderLines)
		{
			OrderLines.Clear();

			foreach (var ol in orderLines)
			{
				if (ol.Count != 0)
				{
					OrderLines.Add(ol);
				}
			}
		}

		/// <summary>
		/// Gets the order lines.
		/// </summary>
		/// <returns>The order lines.</returns>
		public List<IOrderLine> GetOrderLines()
		{
			return OrderLines.Convert(x => x as IOrderLine);
		}

		/// <summary>
		/// Order lines count.
		/// </summary>
		/// <returns>The lines count.</returns>
		public int OrderLinesCount()
		{
			return OrderLines.Count;
		}

		/// <summary>
		/// Total.
		/// </summary>
		/// <returns>Total sum.</returns>
		public int Total()
		{
			var total = 0;

			foreach (var ol in OrderLines)
			{
				total += ol.Count * ol.Price;
			}

			return total;
		}
	}
}