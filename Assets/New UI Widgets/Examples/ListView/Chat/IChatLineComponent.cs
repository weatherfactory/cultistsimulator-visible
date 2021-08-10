namespace UIWidgets.Examples
{
	using UnityEngine;

	/// <summary>
	/// ChatLineComponent interface.
	/// </summary>
	public interface IChatLineComponent
	{
		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		void SetData(ChatLine item);

		/// <summary>
		/// Create component instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		/// <returns>ChatLineComponent instance.</returns>
		IChatLineComponent IInstance(Transform parent);

		/// <summary>
		/// Free used instance.
		/// </summary>
		/// <param name="parent">New parent.</param>
		void Free(Transform parent);
	}
}