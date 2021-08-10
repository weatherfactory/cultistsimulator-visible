namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Item class for GroupedPhotos.
	/// </summary>
	public class Photo
	{
		/// <summary>
		/// Date.
		/// </summary>
		public DateTime Created;

		/// <summary>
		/// Image.
		/// </summary>
		public Sprite Image;

		/// <summary>
		/// Is group?
		/// </summary>
		public bool IsGroup;

		/// <summary>
		/// Is empty?
		/// </summary>
		public bool IsEmpty;

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString()
		{
			return string.Format("{0} at {1:yyyy-MM-dd}; IsGroup: {2}; IsEmpty: {3}", Image, Created, IsGroup, IsEmpty);
		}
	}
}