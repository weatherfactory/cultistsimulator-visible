namespace UIWidgets.Styles
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Style will be applied to specified object.
	/// Should be used for composite widgets when inner widget not styled automatically like Table with TableHeader.
	/// </summary>
	[Serializable]
	public class StyleSupportAny : MonoBehaviour, IStylable
	{
		/// <summary>
		/// GameObject to apply style.
		/// </summary>
		[SerializeField]
		public GameObject Object;

		#region IStylable implementation

		/// <summary>
		/// Set the style.
		/// </summary>
		/// <returns><c>true</c>, if style was set, <c>false</c> otherwise.</returns>
		/// <param name="style">Style.</param>
		public virtual bool SetStyle(Style style)
		{
			style.ApplyTo(Object);

			return true;
		}
		#endregion
	}
}