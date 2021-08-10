namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test Switch.
	/// </summary>
	public class SwitchTest : MonoBehaviour
	{
		/// <summary>
		/// Switch.
		/// </summary>
		[SerializeField]
		protected Switch Switch;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			if (Switch != null)
			{
				Switch.OnValueChanged.AddListener(OnSwitchChanged);
			}
		}

		/// <summary>
		/// Handle switch changed event.
		/// </summary>
		/// <param name="status">Status.</param>
		protected virtual void OnSwitchChanged(bool status)
		{
			Debug.Log("switch status: " + status);
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Switch != null)
			{
				Switch.OnValueChanged.RemoveListener(OnSwitchChanged);
			}
		}
	}
}