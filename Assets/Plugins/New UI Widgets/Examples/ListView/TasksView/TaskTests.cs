namespace UIWidgets.Examples.Tasks
{
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// Task test.
	/// </summary>
	public class TaskTests : MonoBehaviour
	{
		/// <summary>
		/// TaskView.
		/// </summary>
		public TaskView Tasks;

		/// <summary>
		/// Add task.
		/// </summary>
		public void AddTask()
		{
			var task = new Task() { Name = "Random Task", Progress = 0 };

			Tasks.DataSource.Add(task);

			StartCoroutine(UpdateProgress(task, 1f, Random.Range(1, 10)));
		}

		IEnumerator UpdateProgress(Task task, float time, int delta)
		{
			while (task.Progress < 100)
			{
				yield return new WaitForSeconds(time);
				task.Progress = Mathf.Min(task.Progress + delta, 100);
			}
		}
	}
}