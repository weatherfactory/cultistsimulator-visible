namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TileViewIcons DataSource.
	/// </summary>
	[RequireComponent(typeof(TileViewIcons))]
	public class TileViewIconsDataSource : MonoBehaviour
	{
		/// <summary>
		/// Icons.
		/// </summary>
		[SerializeField]
		protected List<Sprite> Icons;

		/// <summary>
		/// Generate and set TileViewIcons DataSource.
		/// </summary>
		protected virtual void Start()
		{
			var n = Icons.Count - 1;
			var tiles = GetComponent<TileViewIcons>();
			tiles.Init();

			tiles.DataSource = UtilitiesCollections.CreateList(140, x => new ListViewIconsItemDescription()
			{
				Name = "Tile " + x,
				Icon = Icons.Count > 0 ? Icons[Random.Range(0, n)] : null,
			});
		}
	}
}