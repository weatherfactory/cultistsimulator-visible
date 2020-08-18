namespace UIWidgets
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// Connector position.
	/// </summary>
	public enum ConnectorPosition
	{
		// Nearest = 0,

		/// <summary>
		/// Left.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Right.
		/// </summary>
		Right = 2,

		/// <summary>
		/// Top.
		/// </summary>
		Top = 3,

		/// <summary>
		/// Bottom.
		/// </summary>
		Bottom = 4,

		/// <summary>
		/// Center.
		/// </summary>
		Center = 5,
	}

	/// <summary>
	/// Connector arrow.
	/// </summary>
	public enum ConnectorArrow
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,

		// Forward = 1,
		// Backward = 2,
		// Bidirectional = 3,
	}

	/// <summary>
	/// Connector type.
	/// </summary>
	public enum ConnectorType
	{
		/// <summary>
		/// Straight line.
		/// </summary>
		Straight = 0,

		// Bezier = 1,
	}

	/// <summary>
	/// Connector line.
	/// </summary>
	[Serializable]
	public class ConnectorLine : INotifyPropertyChanged
	{
		[SerializeField]
		ConnectorPosition start = ConnectorPosition.Right;

		/// <summary>
		/// Gets or sets the start.
		/// </summary>
		/// <value>The start.</value>
		public ConnectorPosition Start
		{
			get
			{
				return start;
			}

			set
			{
				start = value;
				Changed("Start");
			}
		}

		[SerializeField]
		RectTransform target;

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>The target.</value>
		public RectTransform Target
		{
			get
			{
				return target;
			}

			set
			{
				target = value;
				Changed("Target");
			}
		}

		[SerializeField]
		ConnectorPosition end = ConnectorPosition.Left;

		/// <summary>
		/// Gets or sets the end.
		/// </summary>
		/// <value>The end.</value>
		public ConnectorPosition End
		{
			get
			{
				return end;
			}

			set
			{
				end = value;
				Changed("End");
			}
		}

		[SerializeField]
		ConnectorArrow arrow = ConnectorArrow.None;

		/// <summary>
		/// Gets or sets the arrow.
		/// </summary>
		/// <value>The arrow.</value>
		public ConnectorArrow Arrow
		{
			get
			{
				return arrow;
			}

			set
			{
				arrow = value;
				Changed("Arrow");
			}
		}

		[SerializeField]
		ConnectorType type;

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		public ConnectorType Type
		{
			get
			{
				return type;
			}

			set
			{
				type = value;
				Changed("Type");
			}
		}

		[SerializeField]
		float thickness = 1f;

		/// <summary>
		/// Gets or sets the thickness.
		/// </summary>
		/// <value>The thickness.</value>
		public float Thickness
		{
			get
			{
				return thickness;
			}

			set
			{
				thickness = value;
				Changed("Thickness");
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = Utilities.DefaultPropertyHandler;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event OnChange OnChange = Utilities.DefaultHandler;

		/// <summary>
		/// Changed the specified propertyName.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void Changed(string propertyName)
		{
			OnChange();
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}