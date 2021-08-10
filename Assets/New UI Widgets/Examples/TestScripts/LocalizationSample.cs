namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Sample script how to add localization for ListViewIcons, ListViewCustom, TileView, TreeView.
	/// </summary>
	public class LocalizationSample : MonoBehaviour
	{
		[SerializeField]
		ListViewIcons targetListViewIcons;

		/// <summary>
		/// Target ListViewIcons.
		/// </summary>
		public ListViewIcons TargetListViewIcons
		{
			get
			{
				if (targetListViewIcons == null)
				{
					targetListViewIcons = GetComponent<ListViewIcons>();
				}

				return targetListViewIcons;
			}
		}

		/// <summary>
		/// Start this instance and adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			TargetListViewIcons.Init();

			Localize();

			// Add callback on language change, if localization system support this.
			// LocalizationSystem.OnLanguageChange += Localize;
			// LocalizationSystem.OnLanguageChange.AddListener(Localize);
		}

		/// <summary>
		/// Localize data source.
		/// </summary>
		public void Localize()
		{
			// get localized strings, instead GetLocalizedString() use similar function from localization system
			TargetListViewIcons.DataSource.ForEach(x => x.LocalizedName = GetLocalizedString(x.Name));

			// update items in ListViewIcons
			// - or -
			// update ListViewCustom, TileView, TreeViewCustom
			// don't need to call for TreeView or simple ListView
			TargetListViewIcons.UpdateItems();
		}

		static string GetLocalizedString(string str)
		{
			return str;
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			// Remove callback on language change, if localization system support this.
			// LocalizationSystem.OnLanguageChange -= Localize;
			// LocalizationSystem.OnLanguageChange.RemoveListener(Localize);
		}
	}
}