namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Style will not be apllied for children gameobjects.
	/// </summary>
	[Serializable]
	public class StyleSupportStop : MonoBehaviour, IStylable
	{
		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			return true;
		}
		#endregion
	}
}