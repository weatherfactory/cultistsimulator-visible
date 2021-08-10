namespace UIWidgets
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// Scale mark.
	/// </summary>
	[Serializable]
	public class ScaleMark : INotifyPropertyChanged
	{
		[SerializeField]
		float step;

		/// <summary>
		/// Step.
		/// </summary>
		public float Step
		{
			get
			{
				return step;
			}

			set
			{
				if (step != value)
				{
					step = value;
					Changed("Step");
				}
			}
		}

		[SerializeField]
		ScaleMarkTemplate template;

		/// <summary>
		/// Template.
		/// </summary>
		public ScaleMarkTemplate Template
		{
			get
			{
				return template;
			}

			set
			{
				if (template != value)
				{
					template = value;
					Changed("Template");
				}
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = Utilities.DefaultPropertyHandler;

		/// <summary>
		/// Raise PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void Changed(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}