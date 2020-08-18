namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Tab component.
	/// </summary>
	public class TabButtonComponentBase : MonoBehaviour, IUpgradeable
	{
		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		public TextAdapter NameAdapter;

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void SetButtonData(Tab tab)
		{
			NameAdapter.text = tab.Name;
		}

		/// <summary>
		/// Upgrade this instance.
		/// </summary>
		public virtual void Upgrade()
		{
		}

#if UNITY_EDITOR
		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected virtual void OnValidate()
		{
			if (!Compatibility.IsPrefab(this))
			{
				Upgrade();
			}
		}
#endif
	}
}