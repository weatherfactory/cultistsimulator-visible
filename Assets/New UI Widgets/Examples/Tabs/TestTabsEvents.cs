namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test Tabs events.
	/// </summary>
	public class TestTabsEvents : MonoBehaviour
	{
		/// <summary>
		/// Tabs.
		/// </summary>
		[SerializeField]
		protected TabsIcons Tabs;

		int currentTabIndex;

		/// <summary>
		/// Process start.
		/// </summary>
		protected void Start()
		{
			currentTabIndex = Array.IndexOf(Tabs.TabObjects, Tabs.SelectedTab);

			Tabs.OnTabSelect.AddListener(TabChanged);
		}

		void TabChanged(int newTabIndex)
		{
			Debug.Log("deselected tab: " + GetTabName(currentTabIndex) + "; index " + currentTabIndex);
			Debug.Log("selected tab: " + GetTabName(newTabIndex) + "; index " + newTabIndex);

			currentTabIndex = newTabIndex;
		}

		string GetTabName(int index)
		{
			if (index < 0 || index >= Tabs.TabObjects.Length)
			{
				return "none";
			}

			return Tabs.TabObjects[index].Name;
		}

		/// <summary>
		/// Process destroy.
		/// </summary>
		protected void OnDestroy()
		{
			Tabs.OnTabSelect.RemoveListener(TabChanged);
		}
	}
}