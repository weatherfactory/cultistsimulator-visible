namespace UIWidgets
{
	using System.Collections;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// ProgressbarIndeterminate.
	/// http://ilih.ru/images/unity-assets/UIWidgets/ProgressbarIndeterminate.png
	/// </summary>
	public class ProgressbarIndeterminate : MonoBehaviour, IStylable
	{
		/// <summary>
		/// Progressbar animation direction.
		/// </summary>
		[SerializeField]
		public ProgressbarDirection Direction = ProgressbarDirection.Horizontal;

		/// <summary>
		/// The indeterminate bar.
		/// Use texture type "texture" and set wrap mode = repeat;
		/// </summary>
		[SerializeField]
		public RawImage Bar;

		/// <summary>
		/// Border.
		/// </summary>
		[SerializeField]
		public Image Border;

		/// <summary>
		/// Mask.
		/// </summary>
		[SerializeField]
		public Image Mask;

		/// <summary>
		/// The speed.
		/// For Indeterminate speed of changing uvRect coordinates.
		/// </summary>
		[SerializeField]
		public float Speed = 0.1f;

		/// <summary>
		/// The unscaled time.
		/// </summary>
		[SerializeField]
		public bool UnscaledTime = false;

		/// <summary>
		/// Gets a value indicating whether this instance is animation run.
		/// </summary>
		/// <value><c>true</c> if this instance is animation run; otherwise, <c>false</c>.</value>
		public bool IsAnimationRunning
		{
			get;
			protected set;
		}

		IEnumerator currentAnimation;

		/// <summary>
		/// Animate the progressbar to specified targetValue.
		/// </summary>
		public void Animate()
		{
			if (currentAnimation != null)
			{
				StopCoroutine(currentAnimation);
			}

			currentAnimation = IndeterminateAnimation();

			IsAnimationRunning = true;
			StartCoroutine(currentAnimation);
		}

		/// <summary>
		/// Stop animation.
		/// </summary>
		public void Stop()
		{
			if (IsAnimationRunning)
			{
				StopCoroutine(currentAnimation);
				IsAnimationRunning = false;
			}
		}

		/// <summary>
		/// Gets the time.
		/// </summary>
		/// <returns>The time.</returns>
		[System.Obsolete("Use Utilities.GetTime(UnscaledTime).")]
		protected virtual float GetTime()
		{
			return Utilities.GetTime(UnscaledTime);
		}

		IEnumerator IndeterminateAnimation()
		{
			while (true)
			{
				var r = Bar.uvRect;
				if (Direction == ProgressbarDirection.Horizontal)
				{
					r.x = (Utilities.GetTime(UnscaledTime) * (-Speed)) % 1;
				}
				else
				{
					r.y = (Utilities.GetTime(UnscaledTime) * (-Speed)) % 1;
				}

				Bar.uvRect = r;
				yield return null;
			}
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			style.ProgressbarIndeterminate.Texture.ApplyTo(Bar);

			style.ProgressbarIndeterminate.Mask.ApplyTo(Mask);
			style.ProgressbarIndeterminate.Border.ApplyTo(Border);

			return true;
		}
		#endregion
	}
}