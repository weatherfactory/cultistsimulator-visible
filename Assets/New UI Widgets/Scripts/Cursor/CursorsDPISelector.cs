namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Cursors selector by DPI.
	/// </summary>
	public class CursorsDPISelector : MonoBehaviour
	{
		/// <summary>
		/// Cursors for the specified DPI.
		/// </summary>
		[Serializable]
		public class CursorsDPI
		{
			/// <summary>
			/// Cursors.
			/// </summary>
			[SerializeField]
			public Cursors Cursors;

			/// <summary>
			/// DPI.
			/// </summary>
			[SerializeField]
			public float DPI;
		}

		/// <summary>
		/// Default cursors.
		/// </summary>
		[SerializeField]
		public Cursors DefaultCursors;

		/// <summary>
		/// Cursors per DPI.
		/// </summary>
		[SerializeField]
		public List<CursorsDPI> CursorsPerDPI = new List<CursorsDPI>();

		/// <summary>
		/// Current DPI.
		/// </summary>
		protected float currentDPI;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected virtual void Start()
		{
			DPIChanged(Screen.dpi);
		}

		/// <summary>
		/// DPI changed.
		/// </summary>
		/// <param name="dpi">DPI.</param>
		protected virtual void DPIChanged(float dpi)
		{
			var current = DefaultCursors;
			var current_diff = float.MaxValue;

			foreach (var c in CursorsPerDPI)
			{
				var diff = Mathf.Abs(c.DPI - dpi);
				if (diff < current_diff)
				{
					current = c.Cursors;
					diff = current_diff;
				}
			}

			currentDPI = dpi;
			UICursor.Cursors = current;
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			if (currentDPI != Screen.dpi)
			{
				DPIChanged(Screen.dpi);
			}
		}
	}
}