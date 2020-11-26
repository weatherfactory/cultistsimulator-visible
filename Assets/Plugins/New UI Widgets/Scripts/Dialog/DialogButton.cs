namespace UIWidgets
{
	using System;

	/// <summary>
	/// Dialog button info.
	/// </summary>
	public class DialogButton
	{
		/// <summary>
		/// Label.
		/// </summary>
		public string Label;

		Func<int, bool> action;

		/// <summary>
		/// Action with button index.
		/// </summary>
		public Func<int, bool> Action
		{
			get
			{
				return action;
			}

			set
			{
				action = value;
				actionBool = ActionBoolProxy;
			}
		}

		Func<bool> actionBool;

		/// <summary>
		/// Action without button index.
		/// Exists only to keep compatibility with previous versions.
		/// </summary>
		[Obsolete("Replaced with Action.")]
		public Func<bool> ActionBool
		{
			get
			{
				return actionBool;
			}

			set
			{
				action = ActionProxy;
				actionBool = value;
			}
		}

		/// <summary>
		/// Template index.
		/// </summary>
		public int TemplateIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.DialogButton"/> class.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="action">Action on button click.</param>
		/// <param name="templateIndex">Button template index.</param>
		public DialogButton(string label, Func<int, bool> action, int templateIndex = 0)
		{
			Label = label;
			Action = action;
			TemplateIndex = templateIndex;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.DialogButton"/> class.
		/// Exists only to keep compatibility with previous versions.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="action">Action on button click.</param>
		/// <param name="templateIndex">Button template index.</param>
		[Obsolete("Type of \"action\" parameter changed to Func<int, bool> from Func<bool>")]
		public DialogButton(string label, Func<bool> action, int templateIndex = 0)
		{
			Label = label;
			ActionBool = action;
			TemplateIndex = templateIndex;
		}

		/// <summary>
		/// Action proxy.
		/// </summary>
		/// <param name="index">Index of the button.</param>
		/// <returns>true; if dialog should be closed; otherwise false.</returns>
		protected bool ActionProxy(int index)
		{
			return actionBool();
		}

		/// <summary>
		/// Action proxy.
		/// </summary>
		/// <returns>true; if dialog should be closed; otherwise false.</returns>
		protected bool ActionBoolProxy()
		{
			return action(0);
		}
	}
}