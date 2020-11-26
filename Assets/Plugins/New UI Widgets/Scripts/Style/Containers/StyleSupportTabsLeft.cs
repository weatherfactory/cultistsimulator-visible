namespace UIWidgets.Styles
{
	using UnityEngine;

	/// <summary>
	/// Style support for the tabs on left.
	/// </summary>
	[RequireComponent(typeof(IStylable<StyleTabs>))]
	public class StyleSupportTabsLeft : MonoBehaviour, IStylable
	{
		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public virtual bool SetStyle(Style style)
		{
			var component = Compatibility.GetComponent<IStylable<StyleTabs>>(gameObject);
			if (component != null)
			{
				component.SetStyle(style.TabsLeft, style);
			}

			return true;
		}
		#endregion
	}
}