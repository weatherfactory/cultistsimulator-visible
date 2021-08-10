namespace EasyLayoutNS
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// EasyLayout utilities.
	/// </summary>
	public static class EasyLayoutUtilities
	{
		/// <summary>
		/// Maximum count of items in group.
		/// </summary>
		/// <param name="group">Group.</param>
		/// <returns>Maximum count.</returns>
		public static int MaxCount(List<List<LayoutElementInfo>> group)
		{
			int result = 0;

			for (int i = 0; i < group.Count; i++)
			{
				result = Mathf.Max(result, group[i].Count);
			}

			return result;
		}

		/// <summary>
		/// Transpose the specified group.
		/// </summary>
		/// <param name="group">Group.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <returns>Transposed list.</returns>
		public static List<List<T>> Transpose<T>(List<List<T>> group)
		{
			var result = new List<List<T>>();

			for (int i = 0; i < group.Count; i++)
			{
				for (int j = 0; j < group[i].Count; j++)
				{
					if (result.Count <= j)
					{
						result.Add(new List<T>());
					}

					result[j].Add(group[i][j]);
				}
			}

			return result;
		}

		/// <summary>
		/// Get scaled width.
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="ui">User interface.</param>
		public static float ScaledWidth(RectTransform ui)
		{
			return ui.rect.width * ui.localScale.x;
		}

		/// <summary>
		/// Get scaled height.
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="ui">User interface.</param>
		public static float ScaledHeight(RectTransform ui)
		{
			return ui.rect.height * ui.localScale.y;
		}
	}
}