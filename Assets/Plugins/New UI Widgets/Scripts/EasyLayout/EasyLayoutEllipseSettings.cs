namespace EasyLayoutNS
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// Settings for the staggered layout.
	/// </summary>
	[Serializable]
	public class EasyLayoutEllipseSettings : INotifyPropertyChanged
	{
		[SerializeField]
		private bool widthAuto = true;

		/// <summary>
		/// Calculate with or not.
		/// </summary>
		public bool WidthAuto
		{
			get
			{
				return widthAuto;
			}

			set
			{
				if (widthAuto != value)
				{
					widthAuto = value;
					Changed("WidthAuto");
				}
			}
		}

		[SerializeField]
		private float width;

		/// <summary>
		/// Width.
		/// </summary>
		public float Width
		{
			get
			{
				return width;
			}

			set
			{
				if (width != value)
				{
					width = value;
					Changed("Width");
				}
			}
		}

		[SerializeField]
		private bool heightAuto = true;

		/// <summary>
		/// Calculate height or not.
		/// </summary>
		public bool HeightAuto
		{
			get
			{
				return heightAuto;
			}

			set
			{
				if (heightAuto != value)
				{
					heightAuto = value;
					Changed("HeightAuto");
				}
			}
		}

		[SerializeField]
		private float height;

		/// <summary>
		/// Height.
		/// </summary>
		public float Height
		{
			get
			{
				return height;
			}

			set
			{
				if (height != value)
				{
					height = value;
					Changed("Height");
				}
			}
		}

		[SerializeField]
		private float angleStart;

		/// <summary>
		/// Angle for the display first element.
		/// </summary>
		public float AngleStart
		{
			get
			{
				return angleStart;
			}

			set
			{
				if (angleStart != value)
				{
					angleStart = value;
					Changed("AngleStart");
				}
			}
		}

		[SerializeField]
		private bool angleStepAuto;

		/// <summary>
		/// Calculate or not AngleStep.
		/// </summary>
		public bool AngleStepAuto
		{
			get
			{
				return angleStepAuto;
			}

			set
			{
				if (angleStepAuto != value)
				{
					angleStepAuto = value;
					Changed("AngleStepAuto");
				}
			}
		}

		[SerializeField]
		private float angleStep = 20f;

		/// <summary>
		/// Angle distance between elements.
		/// </summary>
		public float AngleStep
		{
			get
			{
				return angleStep;
			}

			set
			{
				if (angleStep != value)
				{
					angleStep = value;
					Changed("AngleStep");
				}
			}
		}

		[SerializeField]
		private EllipseFill fill = EllipseFill.Closed;

		/// <summary>
		/// Fill type.
		/// </summary>
		public EllipseFill Fill
		{
			get
			{
				return fill;
			}

			set
			{
				if (fill != value)
				{
					fill = value;
					Changed("Fill");
				}
			}
		}

		[SerializeField]
		private float arcLength = 360f;

		/// <summary>
		/// Arc length.
		/// </summary>
		public float ArcLength
		{
			get
			{
				return arcLength;
			}

			set
			{
				if (arcLength != value)
				{
					arcLength = value;
					Changed("Length");
				}
			}
		}

		[SerializeField]
		private EllipseAlign align;

		/// <summary>
		/// Align.
		/// </summary>
		public EllipseAlign Align
		{
			get
			{
				return align;
			}

			set
			{
				if (align != value)
				{
					align = value;
					Changed("Align");
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private float angleScroll;

		/// <summary>
		/// Angle padding.
		/// </summary>
		public float AngleScroll
		{
			get
			{
				return angleScroll;
			}

			set
			{
				if (angleScroll != value)
				{
					angleScroll = value;
					Changed("AngleScroll");
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private float angleFiller;

		/// <summary>
		/// Angle filler.
		/// </summary>
		public float AngleFiller
		{
			get
			{
				return angleFiller;
			}

			set
			{
				if (angleFiller != value)
				{
					angleFiller = value;
					Changed("AngleFiller");
				}
			}
		}

		[SerializeField]
		private bool elementsRotate = true;

		/// <summary>
		/// Rotate elements.
		/// </summary>
		public bool ElementsRotate
		{
			get
			{
				return elementsRotate;
			}

			set
			{
				if (elementsRotate != value)
				{
					elementsRotate = value;
					Changed("ElementsRotate");
				}
			}
		}

		[SerializeField]
		private float elementsRotationStart;

		/// <summary>
		/// Start rotation for elements.
		/// </summary>
		public float ElementsRotationStart
		{
			get
			{
				return elementsRotationStart;
			}

			set
			{
				if (elementsRotationStart != value)
				{
					elementsRotationStart = value;
					Changed("ElementsRotationStart");
				}
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = EasyLayout.DefaultPropertyHandler;

		/// <summary>
		/// Property changed.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void Changed(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}