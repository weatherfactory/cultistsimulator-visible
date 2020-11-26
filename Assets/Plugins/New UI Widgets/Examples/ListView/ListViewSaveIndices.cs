namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// How to save selected indicies for ListView.
	/// </summary>
	[RequireComponent(typeof(ListViewBase))]
	public class ListViewSaveIndices : MonoBehaviour
	{
		/// <summary>
		/// Key.
		/// </summary>
		[SerializeField]
		public string Key = "Unique Key";

		[SerializeField]
		ListViewBase list;

		/// <summary>
		/// Load saved indicies and adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			list = GetComponent<ListViewBase>();
			list.Init();

			LoadIndices();

			list.OnSelect.AddListener(SaveIndices);
			list.OnDeselect.AddListener(SaveIndices);
		}

		/// <summary>
		/// Load indicies.
		/// </summary>
		protected virtual void LoadIndices()
		{
			if (PlayerPrefs.HasKey(Key))
			{
				var indices = String2Indices(PlayerPrefs.GetString(Key));
				indices.RemoveAll(x => !list.IsValid(x));
				list.SelectedIndices = indices;
			}
		}

		/// <summary>
		/// Save indicies.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="component">Component.</param>
		protected virtual void SaveIndices(int index, ListViewItem component)
		{
			PlayerPrefs.SetString(Key, Indices2String(list.SelectedIndices));
		}

		static List<int> String2Indices(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return new List<int>();
			}

			var result = new List<int>();

			foreach (var index in str.Split(';'))
			{
				result.Add(int.Parse(index));
			}

			return result;
		}

		static string Indices2String(List<int> indices)
		{
			if ((indices == null) || (indices.Count == 0))
			{
				return string.Empty;
			}

			var arr = new string[indices.Count];

			for (int i = 0; i < indices.Count; i++)
			{
				arr[i] = indices[i].ToString();
			}

			return string.Join(";", arr);
		}
	}
}