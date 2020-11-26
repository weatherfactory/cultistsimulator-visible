namespace UIWidgets
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Wrapper for the Cursor class.
	/// Required to support multiple behavior components on the same GameObject (like Resizable and Rotatable).
	/// </summary>
	public static class UICursor
	{
		static Component currentOwner;

		/// <summary>
		/// Default cursor.
		/// </summary>
		public static Texture2D DefaultCursor;

		/// <summary>
		/// Default cursor hot spot.
		/// </summary>
		public static Vector2 DefaultCursorHotSpot;

		/// <summary>
		/// Can the specified component set cursor?
		/// </summary>
		/// <param name="owner">Component.</param>
		/// <returns>true if component can set cursor; otherwise false.</returns>
		public static bool CanSet(Component owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}

			if (currentOwner == null)
			{
				return true;
			}

			return owner == currentOwner;
		}

		/// <summary>
		/// Set cursor.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="texture">Cursor texture.</param>
		/// <param name="hotspot">Cursor hot spot.</param>
		public static void Set(Component owner, Texture2D texture, Vector2 hotspot)
		{
			if (!CanSet(owner))
			{
				return;
			}

			currentOwner = owner;
			Cursor.SetCursor(texture, hotspot, Compatibility.GetCursorMode());
		}

		/// <summary>
		/// Reset cursor.
		/// </summary>
		/// <param name="owner">Owner.</param>
		public static void Reset(Component owner)
		{
			if (!CanSet(owner))
			{
				return;
			}

			currentOwner = null;
			Cursor.SetCursor(DefaultCursor, DefaultCursorHotSpot, Compatibility.GetCursorMode());
		}
	}
}