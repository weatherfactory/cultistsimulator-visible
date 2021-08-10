namespace UIWidgets.Examples
{
	using System.Collections;
	using UIWidgets;
	using UIWidgets.Extensions;
	using UnityEngine;

	/// <summary>
	/// Test ScrollRect Events.
	/// </summary>
	[RequireComponent(typeof(ScrollRectEvents))]
	public class TestScrollRectEvents : MonoBehaviour
	{
		/// <summary>
		/// ListView.
		/// </summary>
		[SerializeField]
		public ListViewIcons ListView;

		/// <summary>
		/// Data
		/// </summary>
		protected ObservableList<ListViewIconsItemDescription> Data;

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

#pragma warning disable 0618
			ListView.Sort = false;
#pragma warning restore 0618
			Data = ListView.DataSource;
			Data.Comparison = null;
			ListView.Init();

			var scrollRectEvents = GetComponent<ScrollRectEvents>();
			if (scrollRectEvents != null)
			{
				scrollRectEvents.OnPullUp.AddListener(Refresh);
				scrollRectEvents.OnPullDown.AddListener(LoadMore);
			}
		}

		/// <summary>
		/// Handle enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			Init();
			StartCoroutine(LoadData(0));
		}

		/// <summary>
		/// Load data.
		/// </summary>
		/// <param name="start">Start index.</param>
		/// <returns>Coroutine.</returns>
		protected virtual IEnumerator LoadData(int start)
		{
			if (start == 0)
			{
				Data.Clear();
			}

			var lines = Compatibility.EmptyArray<string>();

			var url = "https://ilih.ru/steamspy/?start=" + start.ToString();
#if UNITY_2018_3_OR_NEWER
			using (var www = UnityEngine.Networking.UnityWebRequest.Get(new System.Uri(url)))
			{
				yield return www.SendWebRequest();

				if (Compatibility.IsError(www))
				{
					Debug.Log(www.error);
				}
				else
				{
					lines = www.downloadHandler.text.Split('\n');
				}
			}
#else
			WWW www = new WWW(url);
			yield return www;

			lines = www.text.Split('\n');

			www.Dispose();
#endif

			Data.BeginUpdate();

			lines.ForEach(ParseLine);

			Data.EndUpdate();
		}

		/// <summary>
		/// Parse line.
		/// </summary>
		/// <param name="line">Line.</param>
		protected virtual void ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return;
			}

			var info = line.Split('\t');

			var item = new ListViewIconsItemDescription() { Name = string.Format("{0}. {1}", Data.Count + 1, info[0]), };
			Data.Add(item);
		}

		/// <summary>
		/// Load initial data.
		/// </summary>
		public void Refresh()
		{
			StartCoroutine(LoadData(0));
		}

		/// <summary>
		/// Load more data.
		/// </summary>
		public void LoadMore()
		{
			StartCoroutine(LoadData(Data.Count));
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			var scrollRectEvents = GetComponent<ScrollRectEvents>();
			if (scrollRectEvents != null)
			{
				scrollRectEvents.OnPullUp.RemoveListener(Refresh);
				scrollRectEvents.OnPullDown.RemoveListener(LoadMore);
			}
		}
	}
}