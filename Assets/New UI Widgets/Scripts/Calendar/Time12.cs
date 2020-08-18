namespace UIWidgets
{
	using System;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Time widget with 12 hour format.
	/// </summary>
	public class Time12 : Time24, IUpgradeable
	{
		/// <summary>
		/// The AMPM button.
		/// </summary>
		[SerializeField]
		protected Button AMPMButton;

		/// <summary>
		/// The AMPM text.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		[Obsolete("Replaced with AMPMTextAdapter")]
		protected Text AMPMText;

		/// <summary>
		/// The AMPM text.
		/// </summary>
		[SerializeField]
		protected TextAdapter ampmTextAdapter;

		/// <summary>
		/// The AMPM text.
		/// </summary>
		public TextAdapter AMPMTextAdapter
		{
			get
			{
				return ampmTextAdapter;
			}

			set
			{
				ampmTextAdapter = value;
				UpdateInputs();
			}
		}

		/// <summary>
		/// Adds the listeners.
		/// </summary>
		protected override void AddListeners()
		{
			base.AddListeners();

			if (AMPMButton != null)
			{
				AMPMButton.onClick.AddListener(ToggleAMPM);
			}
		}

		/// <summary>
		/// Removes the listeners.
		/// </summary>
		protected override void RemoveListeners()
		{
			base.RemoveListeners();

			if (AMPMButton != null)
			{
				AMPMButton.onClick.RemoveListener(ToggleAMPM);
			}
		}

		/// <summary>
		/// Toggles the AMPM.
		/// </summary>
		public virtual void ToggleAMPM()
		{
			Time += new TimeSpan(12, 0, 0);
		}

		/// <summary>
		/// Updates the inputs.
		/// </summary>
		public override void UpdateInputs()
		{
			if (InputMinutesAdapter != null)
			{
				InputMinutesAdapter.text = Time.Minutes.ToString("D2");
			}

			if (InputSecondsAdapter != null)
			{
				InputSecondsAdapter.text = Time.Seconds.ToString("D2");
			}

			var hours = Time.Hours;

			if (AMPMTextAdapter != null)
			{
				AMPMTextAdapter.text = hours < 12 ? "AM" : "PM";
			}

			if (InputHoursAdapter != null)
			{
				if (hours == 0)
				{
					hours = 12;
				}
				else if (hours >= 13)
				{
					hours -= 12;
				}

				InputHoursAdapter.text = hours.ToString("D2");
			}
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public override void Upgrade()
		{
			base.Upgrade();

#pragma warning disable 0618
			Utilities.GetOrAddComponent(AMPMText, ref ampmTextAdapter);
#pragma warning restore 0618
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();

			if (!Compatibility.IsPrefab(this))
			{
				Upgrade();
			}
		}
#endif

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public override bool SetStyle(Style style)
		{
			base.SetStyle(style);

			if (AMPMButton != null)
			{
				style.Time.AMPMBackground.ApplyTo(AMPMButton);
			}

			if (AMPMTextAdapter != null)
			{
				style.Time.AMPMText.ApplyTo(AMPMTextAdapter.gameObject);
			}

			return true;
		}
		#endregion
	}
}