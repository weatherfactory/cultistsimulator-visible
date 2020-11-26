namespace UIWidgets.Examples
{
	using System.Collections;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// SteamSpyData.
	/// </summary>
	public class SteamSpyData : MonoBehaviour
	{
		/// <summary>
		/// SteamSpyView.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("steamSpyView")]
		protected SteamSpyView SteamSpyView;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			Load();
		}

		/// <summary>
		/// Load data.
		/// </summary>
		public void Load()
		{
			StartCoroutine(LoadData());
		}

		/// <summary>
		/// Coroutine to load data from web.
		/// </summary>
		/// <returns>IEnumerator.</returns>
		protected IEnumerator LoadData()
		{
			var lines = new string[] { };

			var url = "https://ilih.ru/steamspy/";
#if UNITY_2018_3_OR_NEWER
			using (var www = UnityEngine.Networking.UnityWebRequest.Get(url))
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
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

			SteamSpyView.DataSource.BeginUpdate();

			SteamSpyView.DataSource.Clear();

			lines.ForEach(x =>
			{
				var item = ParseLine(x);
				if (item != null)
				{
					SteamSpyView.DataSource.Add(item);
				}
			});

			SteamSpyView.DataSource.EndUpdate();
		}

		/// <summary>
		/// Parse line and add to SteamSpyView.
		/// </summary>
		/// <param name="line">Line to parse.</param>
		/// <returns>Item.</returns>
		protected static SteamSpyItem ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return null;
			}

			var info = line.Split('\t');

			var item = new SteamSpyItem()
			{
				Name = info[0],
				ScoreRank = string.IsNullOrEmpty(info[1]) ? -1 : int.Parse(info[1]),

				Owners = int.Parse(info[2]),
				OwnersVariance = int.Parse(info[3]),

				Players = int.Parse(info[4]),
				PlayersVariance = int.Parse(info[5]),

				PlayersIn2Week = int.Parse(info[6]),
				PlayersIn2WeekVariance = int.Parse(info[7]),

				AverageTimeIn2Weeks = int.Parse(info[8]),
				MedianTimeIn2Weeks = int.Parse(info[9]),
			};

			return item;
		}
	}
}