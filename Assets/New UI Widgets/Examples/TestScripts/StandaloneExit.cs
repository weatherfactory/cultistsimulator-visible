namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// Exit option for standalone build.
	/// </summary>
	public class StandaloneExit : MonoBehaviour
	{
		/// <summary>
		/// Disable gameobject if not standalone build.
		/// </summary>
		protected virtual void Start()
		{
			#if !UNITY_STANDALONE
			gameObject.SetActive(false);
			#endif
		}

		/// <summary>
		/// Quit.
		/// </summary>
		public virtual void Quit()
		{
			Application.Quit();
		}
	}
}