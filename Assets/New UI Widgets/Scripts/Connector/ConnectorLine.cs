namespace UIWidgets
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// Connector line.
	/// </summary>
	[Serializable]
	public class ConnectorLine : INotifyPropertyChanged
	{
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
		[HideInInspector]
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

		[SerializeField]
		float margin = 10f;

		/// <summary>
		/// Gets or sets the margin.
		/// </summary>
		/// <value>The margin.</value>
		public float Margin
		{
			get
			{
				return margin;
			}

			set
			{
				margin = value;
				Changed("Margin");
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