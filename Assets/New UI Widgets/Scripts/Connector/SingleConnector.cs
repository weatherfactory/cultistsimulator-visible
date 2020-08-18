namespace UIWidgets
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Single connector.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("UI/New UI Widgets/Single Connector")]
	public class SingleConnector : ConnectorBase
	{
		/// <summary>
		/// The line.
		/// </summary>
		[SerializeField]
		protected ConnectorLine line;

		/// <summary>
		/// Gets or sets the line.
		/// </summary>
		/// <value>The line.</value>
		public ConnectorLine Line
		{
			get
			{
				return line;
			}

			set
			{
				if (line != null)
				{
					line.OnChange -= LinesChanged;
				}

				line = value;

				if (line != null)
				{
					line.OnChange += LinesChanged;
				}

				LinesChanged();
			}
		}

		/// <summary>
		/// The listener.
		/// </summary>
		protected TransformListener listener;

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected override void Init()
		{
			base.Init();

			Line = line;
		}

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			LinesChanged();
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			RemoveListener();

			base.OnDestroy();
		}

		/// <summary>
		/// Process lines changes.
		/// </summary>
		protected virtual void LinesChanged()
		{
			RemoveListener();
			AddListener();
			SetVerticesDirty();
		}

		/// <summary>
		/// Removes the listener.
		/// </summary>
		protected virtual void RemoveListener()
		{
			if (listener != null)
			{
				listener.OnTransformChanged.RemoveListener(SetVerticesDirty);
			}
		}

		/// <summary>
		/// Adds the listener.
		/// </summary>
		protected virtual void AddListener()
		{
			if ((Line != null) && (Line.Target != null))
			{
				listener = Utilities.GetOrAddComponent<TransformListener>(Line.Target);
				listener.OnTransformChanged.AddListener(SetVerticesDirty);
			}
		}

		#if UNITY_5_3 || UNITY_5_3_OR_NEWER
		/// <summary>
		/// Fill the vertex buffer data.
		/// </summary>
		/// <param name="vh">VertexHelper.</param>
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			AddLine(rectTransform, Line, vh, 0);
		}
		#elif UNITY_5_2
		/// <summary>
		/// Fill the vertex buffer data.
		/// </summary>
		/// <param name="m">Mesh.</param>
		protected override void OnPopulateMesh(Mesh m)
		{
			using (VertexHelper vh = new VertexHelper())
			{
				AddLine(rectTransform, Line, vh, 0);
				vh.FillMesh(m);
			}
		}
		#else
		/// <summary>
		/// Generate vertex for line.
		/// </summary>
		/// <param name="vbo">Vbo.</param>
		protected override void OnFillVBO(List<UIVertex> vbo)
		{
			AddLine(rectTransform, Line, vbo);
		}
		#endif
	}
}