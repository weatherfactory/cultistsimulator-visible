namespace UIWidgets
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// TreeView.DataSource editor window.
	/// </summary>
	public class TreeViewDataSourceWindow : EditorWindow
	{
		Vector2 scrollPos;
		static GameObject currentGameObject;

		/// <summary>
		/// Init.
		/// </summary>
		public static void Init()
		{
			var window = EditorWindow.GetWindow<TreeViewDataSourceWindow>();
			window.Show();
		}

		/// <summary>
		/// Set title.
		/// </summary>
		protected virtual void OnEnable()
		{
			#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
			titleContent = new GUIContent("TreeViewEditor");
			#else
			title = "TreeViewEditor";
			#endif
		}

		/// <summary>
		/// Draw GUI.
		/// </summary>
		protected virtual void OnGUI()
		{
			if (Selection.activeGameObject != null)
			{
				var component = Selection.activeGameObject.GetComponent<TreeViewDataSource>();
				if (component != null)
				{
					currentGameObject = Selection.activeGameObject;
				}
			}

			var data = currentGameObject.GetComponent<TreeViewDataSource>();
			if (data == null)
			{
				GUILayout.Label("Please select TreeView in Hierarchy window.", EditorStyles.boldLabel);
				return;
			}

			var serializedData = new SerializedObject(data);
			var propertyData = serializedData.FindProperty("Data");

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			int prev_depth = -1;

			Rect item_position = new Rect(position)
			{
				x = 4,
				y = 4,
				height = 18,
			};
			item_position.width -= 25;

			var n = propertyData.arraySize;
			for (int i = 0; i < n; i++)
			{
				if (i == n)
				{
					continue;
				}

				var item = propertyData.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				prev_depth = DisplayItem(item, prev_depth, i, item_position, propertyData, ref n);

				EditorGUILayout.EndHorizontal();

				item_position.y += 21;
			}

			EditorGUILayout.EndScrollView();

			FixTree(propertyData, n);

			if (GUILayout.Button("Add Node", GUILayout.Width(100)))
			{
				propertyData.InsertArrayElementAtIndex(propertyData.arraySize);
			}

			serializedData.ApplyModifiedProperties();
		}

		/// <summary>
		/// Fix tree.
		/// </summary>
		/// <param name="list">Nodes list.</param>
		/// <param name="n">Nodes count.</param>
		protected virtual void FixTree(SerializedProperty list, int n)
		{
			int depth = 0;
			for (int i = 0; i < n; i++)
			{
				var sDepth = list.GetArrayElementAtIndex(i).FindPropertyRelative("Depth");
				if (sDepth.intValue < 0)
				{
					sDepth.intValue = 0;
				}

				if ((sDepth.intValue - depth) > 1)
				{
					sDepth.intValue = depth + 1;
				}

				depth = sDepth.intValue;
			}
		}

		/// <summary>
		/// Display item.
		/// </summary>
		/// <param name="item">Node.</param>
		/// <param name="prevDepth">Previous depth level.</param>
		/// <param name="index">Node index.</param>
		/// <param name="itemPosition">Node position.</param>
		/// <param name="list">Nodes list.</param>
		/// <param name="n">Nodes count.</param>
		/// <returns>Depth level.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#", Justification = "Reviewed.")]
		protected virtual int DisplayItem(SerializedProperty item, int prevDepth, int index, Rect itemPosition, SerializedProperty list, ref int n)
		{
			EditorGUI.BeginProperty(itemPosition, new GUIContent(), item);

			var sIsExpanded = item.FindPropertyRelative("IsExpanded");
			var sIcon = item.FindPropertyRelative("Icon");
			var sName = item.FindPropertyRelative("Name");
			var sDepth = item.FindPropertyRelative("Depth");

			var buttons = 6;
			var buttonWidth = 25;
			var nameWidth = itemPosition.width - (20 * sDepth.intValue) - 4 - (buttonWidth + 4) - 140 - ((buttonWidth + 4) * buttons);

			var start = itemPosition.x + (20 * sDepth.intValue);

			var isExpandedRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var iconRect = new Rect(start, itemPosition.y, 140, itemPosition.height);
			start += 140;

			var nameRect = new Rect(start, itemPosition.y, nameWidth, itemPosition.height);
			start += nameWidth;

			var moveLeftRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var moveRightRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var moveUpRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var moveDownRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var addRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);
			start += buttonWidth + 4;

			var deleteRect = new Rect(start, itemPosition.y, buttonWidth, itemPosition.height);

			GUILayout.Space((20 * sDepth.intValue) + 4);

			if (GUILayout.Button(sIsExpanded.boolValue ? "-" : "+", GUILayout.Width(25)))
			{
				sIsExpanded.boolValue = !sIsExpanded.boolValue;
			}

			EditorGUI.LabelField(isExpandedRect, new GUIContent(string.Empty, "Is expanded?"));

			EditorGUI.PropertyField(iconRect, sIcon, GUIContent.none);
			EditorGUI.LabelField(iconRect, new GUIContent(string.Empty, "Icon"));

			EditorGUI.PropertyField(nameRect, sName, GUIContent.none);
			EditorGUI.LabelField(nameRect, new GUIContent(string.Empty, "Name"));

			GUILayout.Space(nameWidth + 148);

			GUI.enabled = sDepth.intValue > 0;
			if (GUILayout.Button("←", GUILayout.Width(buttonWidth)))
			{
				NodeMoveLeft(index, n, list);
			}

			GUI.enabled = true;
			EditorGUI.LabelField(moveLeftRect, new GUIContent(string.Empty, "Move node with subnodes left"));

			GUI.enabled = sDepth.intValue <= prevDepth;
			if (GUILayout.Button("→", GUILayout.Width(buttonWidth)))
			{
				NodeMoveRight(index, n, list);
			}

			GUI.enabled = true;
			EditorGUI.LabelField(moveRightRect, new GUIContent(string.Empty, "Move node with subnodes right"));

			EditorGUI.EndProperty();

			GUI.enabled = index > 0;
			if (GUILayout.Button("↑", GUILayout.Width(buttonWidth)))
			{
				NodeMoveUp(index, n, list);
			}

			GUI.enabled = true;
			EditorGUI.LabelField(moveUpRect, new GUIContent(string.Empty, "Move node with subnodes up"));

			GUI.enabled = (index + 1) < list.arraySize;
			if (GUILayout.Button("↓", GUILayout.Width(buttonWidth)))
			{
				NodeMoveDown(index, n, list);
			}

			GUI.enabled = true;
			EditorGUI.LabelField(moveDownRect, new GUIContent(string.Empty, "Move node with subnodes down"));

			if (GUILayout.Button("+", GUILayout.Width(buttonWidth)))
			{
				list.InsertArrayElementAtIndex(index + 1);
			}

			EditorGUI.LabelField(addRect, new GUIContent(string.Empty, "Add node after current"));

			if (GUILayout.Button("-", GUILayout.Width(buttonWidth)))
			{
				NodeDelete(index, n, list);

				n -= 1;
				return prevDepth;
			}

			EditorGUI.LabelField(deleteRect, new GUIContent(string.Empty, "Delete current node"));

			return sDepth.intValue;
		}

		/// <summary>
		/// Delete node.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		protected virtual void NodeDelete(int index, int listLength, SerializedProperty list)
		{
			int depth = list.GetArrayElementAtIndex(index).FindPropertyRelative("Depth").intValue;
			for (int j = index + 1; j < listLength; j++)
			{
				var child = list.GetArrayElementAtIndex(j);
				if (child.FindPropertyRelative("Depth").intValue <= depth)
				{
					break;
				}

				child.FindPropertyRelative("Depth").intValue -= 1;
			}

			list.DeleteArrayElementAtIndex(index);
		}

		/// <summary>
		/// Move node to left.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		protected virtual void NodeMoveLeft(int index, int listLength, SerializedProperty list)
		{
			ChangeDepth(index, listLength, list, -1);
		}

		/// <summary>
		/// Move node to right.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		protected virtual void NodeMoveRight(int index, int listLength, SerializedProperty list)
		{
			ChangeDepth(index, listLength, list, 1);
		}

		/// <summary>
		/// Change node depth.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		/// <param name="deltaDepth">Depth delta.</param>
		protected virtual void ChangeDepth(int index, int listLength, SerializedProperty list, int deltaDepth)
		{
			if (deltaDepth == 0)
			{
				return;
			}

			var sDepth = list.GetArrayElementAtIndex(index).FindPropertyRelative("Depth");
			var depth = sDepth.intValue;
			for (int j = index + 1; j < listLength; j++)
			{
				var child = list.GetArrayElementAtIndex(j);
				if (child.FindPropertyRelative("Depth").intValue <= depth)
				{
					break;
				}

				child.FindPropertyRelative("Depth").intValue += deltaDepth;
			}

			sDepth.intValue += deltaDepth;
		}

		/// <summary>
		/// Move node to up.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		protected virtual void NodeMoveUp(int index, int listLength, SerializedProperty list)
		{
			var sDepth = list.GetArrayElementAtIndex(index).FindPropertyRelative("Depth");
			var depth = sDepth.intValue;
			var new_depth = (index == 1) ? 0 : list.GetArrayElementAtIndex(index - 2).FindPropertyRelative("Depth").intValue;

			list.MoveArrayElement(index, index - 1);

			for (int j = index + 1; j < listLength; j++)
			{
				var child = list.GetArrayElementAtIndex(j);
				if (child.FindPropertyRelative("Depth").intValue <= depth)
				{
					break;
				}

				list.MoveArrayElement(j, j - 1);
			}

			ChangeDepth(index - 1, listLength, list, new_depth - depth);
		}

		/// <summary>
		/// Move node to down.
		/// </summary>
		/// <param name="index">Node index.</param>
		/// <param name="listLength">Nodes count.</param>
		/// <param name="list">Nodes list.</param>
		protected virtual void NodeMoveDown(int index, int listLength, SerializedProperty list)
		{
			var sDepth = list.GetArrayElementAtIndex(index).FindPropertyRelative("Depth");
			var depth = sDepth.intValue;

			int n = index;
			for (int j = index + 1; j < listLength; j++)
			{
				var child = list.GetArrayElementAtIndex(j);
				if (child.FindPropertyRelative("Depth").intValue <= depth)
				{
					break;
				}

				n += 1;
			}

			for (int j = n; j >= index; j--)
			{
				list.MoveArrayElement(j, j + 1);
			}

			var new_depth = ((n + 1) == listLength) ? 0 : list.GetArrayElementAtIndex(index).FindPropertyRelative("Depth").intValue;
			ChangeDepth(n, listLength, list, new_depth - depth);
		}
	}
}