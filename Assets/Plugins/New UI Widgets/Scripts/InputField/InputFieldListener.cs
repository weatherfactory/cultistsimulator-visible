namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// InputField listener.
	/// </summary>
	public class InputFieldListener : SelectListener, IUpdateSelectedHandler
	{
		/// <summary>
		/// OnMoveEvent.
		/// </summary>
		[SerializeField]
		public MoveEvent OnMoveEvent = new MoveEvent();

		/// <summary>
		/// OnSubmitEvent.
		/// </summary>
		[SerializeField]
		public SubmitEvent OnSubmitEvent = new SubmitEvent();

		/// <summary>
		/// OnUpdateSelected event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnUpdateSelected(BaseEventData eventData)
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				var axisEvent = new AxisEventData(EventSystem.current)
				{
					moveDir = MoveDirection.Left,
				};
				OnMoveEvent.Invoke(axisEvent);
				return;
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				var axisEvent = new AxisEventData(EventSystem.current)
				{
					moveDir = MoveDirection.Right,
				};
				OnMoveEvent.Invoke(axisEvent);
				return;
			}

			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				var axisEvent = new AxisEventData(EventSystem.current)
				{
					moveDir = MoveDirection.Up,
				};
				OnMoveEvent.Invoke(axisEvent);
				return;
			}

			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				var axisEvent = new AxisEventData(EventSystem.current)
				{
					moveDir = MoveDirection.Down,
				};
				OnMoveEvent.Invoke(axisEvent);
				return;
			}

			if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				var isEnter = Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
				var isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				if (!(isEnter && isShift))
				{
					OnSubmitEvent.Invoke(eventData, isEnter);
				}

				return;
			}
		}
	}
}