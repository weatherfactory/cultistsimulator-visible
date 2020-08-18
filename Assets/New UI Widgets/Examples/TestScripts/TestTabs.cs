namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test Tabs.
	/// </summary>
	public class TestTabs : MonoBehaviour
	{
		/// <summary>
		/// Tabs.
		/// </summary>
		[SerializeField]
		protected Tabs Tabs;

		/// <summary>
		/// Adds listeners.
		/// </summary>
		protected virtual void Start()
		{
			Tabs.OnTabSelect.AddListener(Test2);
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Tabs != null)
			{
				Tabs.OnTabSelect.RemoveListener(Test2);
			}
		}

		/// <summary>
		/// Log selected tab info.
		/// </summary>
		/// <param name="index">Index.</param>
		public void Test2(int index)
		{
			Debug.Log("Name: " + Tabs.SelectedTab.Name);
			Debug.Log("Index: " + index);
			Debug.Log("Index: " + Array.IndexOf(Tabs.TabObjects, Tabs.SelectedTab));
		}
	}
}