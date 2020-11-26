namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// UI cursor settings.
	/// </summary>
	public class UICursorSettings : MonoBehaviour
	{
		/// <summary>
		/// Default cursor.
		/// </summary>
		[SerializeField]
		protected Texture2D DefaultCursor;

		/// <summary>
		/// Default cursor hot spot.
		/// </summary>
		[SerializeField]
		protected Vector2 DefaultCursorHotSpot;

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			if (isInited)
			{
				return;
			}

			UICursor.DefaultCursor = DefaultCursor;
			UICursor.DefaultCursorHotSpot = DefaultCursorHotSpot;

			isInited = true;
		}
	}
}