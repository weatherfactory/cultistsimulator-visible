namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Determinate ProgressBar sample.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class SampleProgressBar2 : MonoBehaviour
	{
		/// <summary>
		/// Progressbar.
		/// </summary>
		[SerializeField]
		public ProgressbarDeterminateBase Bar;

		Button button;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			button = GetComponent<Button>();
			if (button != null)
			{
				button.onClick.AddListener(Click);
			}
		}

		/// <summary>
		/// Toggle animation.
		/// </summary>
		protected virtual void Click()
		{
			if (Bar.IsAnimationRunning)
			{
				Bar.Stop();
			}
			else
			{
				if (Bar.Value == 0)
				{
					Bar.Animate(Bar.Max);
				}
				else
				{
					Bar.Animate(0);
				}
			}
		}

		/// <summary>
		/// Remove listener.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (button != null)
			{
				button.onClick.RemoveListener(Click);
			}
		}
	}
}