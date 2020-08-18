namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
#if UNITY_EDITOR
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using UnityEditor;
	using UnityEditor.Events;
#endif

	/// <summary>
	/// Utilities.
	/// </summary>
	public class Utilities
	{
#if UNITY_EDITOR
		/// <summary>
		/// Get friendly name of the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>Friendly name.</returns>
		public static string GetFriendlyTypeName(Type type)
		{
			var friendly_name = type.Name;

			if (type.IsGenericType)
			{
				var backtick_index = friendly_name.IndexOf('`');
				if (backtick_index > 0)
				{
					friendly_name = friendly_name.Remove(backtick_index);
				}

				friendly_name += "<";

				var type_parameters = type.GetGenericArguments();
				for (int i = 0; i < type_parameters.Length; ++i)
				{
					var type_param_name = GetFriendlyTypeName(type_parameters[i]);
					friendly_name += i == 0 ? type_param_name : "," + type_param_name;
				}

				friendly_name += ">";
			}

			return string.IsNullOrEmpty(type.Namespace) ? friendly_name : type.Namespace + "." + friendly_name;
		}

		/// <summary>
		/// Get type by full name.
		/// </summary>
		/// <param name="typename">Type name.</param>
		/// <returns>Type.</returns>
		public static Type GetType(string typename)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var assembly_type in assembly.GetTypes())
				{
					if (assembly_type.FullName == typename)
					{
						return assembly_type;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Creates the object by path to asset prefab.
		/// </summary>
		/// <returns>The created object.</returns>
		/// <param name="path">Path.</param>
		static GameObject CreateObject(string path)
		{
			var prefab = Compatibility.LoadAssetAtPath<GameObject>(path);
			if (prefab == null)
			{
				throw new ArgumentException(string.Format("Prefab not found at path {0}.", path));
			}

			return CreateGameObject(prefab);
		}

		/// <summary>
		/// Create gameobject.
		/// </summary>
		/// <param name="prefab">Prefab.</param>
		/// <param name="undo">Support editor undo.</param>
		/// <returns>Created gameobject.</returns>
		public static GameObject CreateGameObject(GameObject prefab, bool undo = true)
		{
			var go = Compatibility.Instantiate(prefab);

			if (undo)
			{
				Undo.RegisterCreatedObjectUndo(go, "Create " + prefab.name);
			}

			var go_parent = Selection.activeTransform;
			if ((go_parent == null) || ((go_parent.gameObject.transform as RectTransform) == null))
			{
				go_parent = GetCanvasTransform();
			}

			if (go_parent != null)
			{
				if (undo)
				{
					Undo.SetTransformParent(go.transform, go_parent, "Create " + prefab.name);
				}
				else
				{
					go.transform.SetParent(go_parent, false);
				}
			}

			go.name = prefab.name;

			var rectTransform = go.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchoredPosition = new Vector2(0, 0);

				FixInstantiated(prefab, go);
			}

			return go;
		}

		/// <summary>
		/// Returns the asset object of type T with the specified GUID.
		/// </summary>
		/// <param name="guid">GUID.</param>
		/// <returns>Asset with the specified GUID.</returns>
		/// <typeparam name="T">Asset type.</typeparam>
		public static T LoadAssetWithGUID<T>(string guid)
			where T : UnityEngine.Object
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Path not found for the GUID: " + guid);
				return null;
			}

			return Compatibility.LoadAssetAtPath<T>(path);
		}

		/// <summary>
		/// Find assets by specified search.
		/// </summary>
		/// <typeparam name="T">Assets type.</typeparam>
		/// <param name="search">Search string.</param>
		/// <returns>Assets list.</returns>
		public static List<T> GetAssets<T>(string search)
			where T : UnityEngine.Object
		{
			var guids = AssetDatabase.FindAssets(search);

			var result = new List<T>(guids.Length);
			foreach (var guid in guids)
			{
				var asset = LoadAssetWithGUID<T>(guid);
				if (asset != null)
				{
					result.Add(asset);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates the object from asset.
		/// </summary>
		/// <returns>The object from asset.</returns>
		/// <param name="key">Search string.</param>
		static GameObject CreateObjectFromAsset(string key)
		{
			var path = GetAssetPath(key);
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			var go = CreateObject(path);

			Selection.activeObject = go;

			return go;
		}

		/// <summary>
		/// Get asset by label.
		/// </summary>
		/// <typeparam name="T">Asset type.</typeparam>
		/// <param name="label">Asset label.</param>
		/// <returns>Asset.</returns>
		public static T GetAsset<T>(string label)
			where T : UnityEngine.Object
		{
			var path = GetAssetPath(label + "Asset");
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			return Compatibility.LoadAssetAtPath<T>(path);
		}

		/// <summary>
		/// Get asset path by label.
		/// </summary>
		/// <param name="label">Asset label.</param>
		/// <returns>Asset path.</returns>
		public static string GetAssetPath(string label)
		{
			var key = "l:Uiwidgets" + label;
			var guids = AssetDatabase.FindAssets(key);
			if (guids.Length == 0)
			{
				Debug.LogError("Label not found: " + label);
				return null;
			}

			return AssetDatabase.GUIDToAssetPath(guids[0]);
		}

		/// <summary>
		/// Get prefab by label.
		/// </summary>
		/// <param name="label">Prefab label.</param>
		/// <returns>Prefab.</returns>
		public static GameObject GetPrefab(string label)
		{
			var path = GetAssetPath(label + "Prefab");
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			return Compatibility.LoadAssetAtPath<GameObject>(path);
		}

		/// <summary>
		/// Get generated prefab by label.
		/// </summary>
		/// <param name="label">Prefab label.</param>
		/// <returns>Prefab.</returns>
		public static GameObject GetGeneratedPrefab(string label)
		{
			return GetPrefab("Generated" + label);
		}

		/// <summary>
		/// Set prefabs label.
		/// </summary>
		/// <param name="prefab">Prefab.</param>
		public static void SetPrefabLabel(UnityEngine.Object prefab)
		{
			AssetDatabase.SetLabels(prefab, new[] { "UiwidgetsGenerated" + prefab.name + "Prefab", });
		}

		/// <summary>
		/// Create widget template from asset specified by label.
		/// </summary>
		/// <param name="templateLabel">Template label.</param>
		/// <returns>Widget template.</returns>
		public static GameObject CreateWidgetTemplateFromAsset(string templateLabel)
		{
			var path = GetAssetPath(templateLabel + "Prefab");
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			return CreateObject(path);
		}

		/// <summary>
		/// Creates the widget from prefab by name.
		/// </summary>
		/// <param name="widget">Widget name.</param>
		/// <param name="applyStyle">Apply style to created widget.</param>
		/// <param name="converter">Converter for the created widget (mostly used to replace Unity Text with TMPro Text).</param>
		/// <returns>Created GameObject.</returns>
		public static GameObject CreateWidgetFromAsset(string widget, bool applyStyle = true, Action<GameObject> converter = null)
		{
			var go = CreateObjectFromAsset(widget + "Prefab");

			if (go != null)
			{
				if (converter != null)
				{
					converter(go);
				}

				if (applyStyle)
				{
					var style = PrefabsMenu.Instance.DefaultStyle;
					if (style != null)
					{
						style.ApplyTo(go);
					}
				}
			}

			Upgrade(go);

			FixDialogCloseButton(go);

			return go;
		}

		/// <summary>
		/// Creates the widget from prefab by name.
		/// </summary>
		/// <param name="prefab">Widget name.</param>
		/// <param name="applyStyle">Apply style to created widget.</param>
		/// <param name="converter">Converter for the created widget (mostly used to replace Unity Text with TMPro Text).</param>
		/// <returns>Created GameObject.</returns>
		public static GameObject CreateWidgetFromPrefab(GameObject prefab, bool applyStyle = true, Action<GameObject> converter = null)
		{
			var go = CreateGameObject(prefab);

			Selection.activeObject = go;

			if (go != null)
			{
				if (converter != null)
				{
					converter(go);
				}

				if (applyStyle)
				{
					var style = PrefabsMenu.Instance.DefaultStyle;
					if (style != null)
					{
						style.ApplyTo(go);
					}
				}
			}

			Upgrade(go);

			FixDialogCloseButton(go);

			return go;
		}

		static void Upgrade(GameObject go)
		{
			var upgrades = new List<IUpgradeable>();
			Compatibility.GetComponentsInChildren(go.transform, true, upgrades);
			for (int i = 0; i < upgrades.Count; i++)
			{
				upgrades[i].Upgrade();
			}
		}

		/// <summary>
		/// Replace Close button callback on Cancel instead of the Hide for the Dialog components in the specified GameObject.
		/// </summary>
		/// <param name="go">GameObject.</param>
		public static void FixDialogCloseButton(GameObject go)
		{
			var dialogs = go.GetComponentsInChildren<Dialog>(true);

			foreach (var dialog in dialogs)
			{
				var button_go = dialog.transform.Find("Header/CloseButton");
				if (button_go == null)
				{
					continue;
				}

				var button = button_go.GetComponent<Button>();
				if (button == null)
				{
					continue;
				}

				if (IsEventCallMethod(button.onClick, dialog, "Hide"))
				{
					UnityEventTools.RemovePersistentListener(button.onClick, dialog.Hide);
					UnityEventTools.AddPersistentListener(button.onClick, dialog.Cancel);
				}
			}
		}

		static bool IsEventCallMethod(UnityEvent ev, MonoBehaviour target, string method)
		{
			var n = ev.GetPersistentEventCount();
			for (int i = 0; i < n; i++)
			{
				if (ev.GetPersistentMethodName(i) == method)
				{
					if (ev.GetPersistentTarget(i) == target)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the canvas transform.
		/// </summary>
		/// <returns>The canvas transform.</returns>
		public static Transform GetCanvasTransform()
		{
			var canvas = (Selection.activeGameObject != null) ? Selection.activeGameObject.GetComponentInParent<Canvas>() : null;
			if (canvas == null)
			{
				canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
			}

			if (canvas != null)
			{
				return canvas.transform;
			}

			var canvasGO = new GameObject("Canvas")
			{
				layer = LayerMask.NameToLayer("UI"),
			};
			canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGO.AddComponent<CanvasScaler>();
			canvasGO.AddComponent<GraphicRaycaster>();
			Undo.RegisterCreatedObjectUndo(canvasGO, "Create " + canvasGO.name);

			if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
			{
				Compatibility.CreateEventSystem();
			}

			return canvasGO.transform;
		}

		/// <summary>
		/// Serialize object with BinaryFormatter to base64 string.
		/// </summary>
		/// <param name="obj">Object to serialize.</param>
		/// <returns>Serialized string.</returns>
		public static string Serialize(object obj)
		{
			var serializer = new BinaryFormatter();

			using (var ms = new MemoryStream())
			{
				serializer.Serialize(ms, obj);
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		/// <summary>
		/// De-serialize object with BinaryFormatter from base64 string.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="encoded">Serialized string.</param>
		/// <returns>De-serialized object.</returns>
		public static T Deserialize<T>(string encoded)
		{
			var serializer = new BinaryFormatter();
			var ms = new MemoryStream();

			var bytes = Convert.FromBase64String(encoded);
			ms.Write(bytes, 0, bytes.Length);
			ms.Seek(0, SeekOrigin.Begin);

			return (T)serializer.Deserialize(ms);
		}
#endif

		/// <summary>
		/// Function to get time to use with animations.
		/// Can be replaced with custom function.
		/// </summary>
		public static Func<bool, float> GetTime = DefaultGetTime;

		/// <summary>
		/// Function to get unscaled time to use by widgets.
		/// Can be replaced with custom function.
		/// </summary>
		[Obsolete("Replaced with GetTime(true)")]
		public static Func<float> GetUnscaledTime = DefaultGetUnscaledTime;

		/// <summary>
		/// Function to get delta time to use with animations.
		/// Can be replaced with custom function.
		/// </summary>
		public static Func<bool, float> GetDeltaTime = DefaultGetDeltaTime;

		/// <summary>
		/// Default GetTime function from the default Time class.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled time.</param>
		/// <returns>Time.</returns>
		public static float DefaultGetTime(bool unscaledTime)
		{
			return unscaledTime ? Time.unscaledTime : Time.time;
		}

		/// <summary>
		/// Get path to gameobject.
		/// </summary>
		/// <param name="go">GameObject.</param>
		/// <returns>Path.</returns>
		public static string GameObjectPath(GameObject go)
		{
			var parent = go.transform.parent;
			return parent == null ? go.name : GameObjectPath(parent.gameObject) + "/" + go.name;
		}

		/// <summary>
		/// Default GetUnscaledTime function.
		/// </summary>
		/// <returns>Default Time.unscaledTime.</returns>
		public static float DefaultGetUnscaledTime()
		{
			return Time.unscaledTime;
		}

		/// <summary>
		/// Default GetDeltaTime function from the default Time class.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled delta time.</param>
		/// <returns>Delta Time.</returns>
		public static float DefaultGetDeltaTime(bool unscaledTime)
		{
			return unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		}

		/// <summary>
		/// Create list.
		/// </summary>
		/// <typeparam name="T">Type of the item.</typeparam>
		/// <param name="count">Items count.</param>
		/// <param name="create">Function to create item.</param>
		/// <returns>List.</returns>
		public static ObservableList<T> CreateList<T>(int count, Func<int, T> create)
		{
			var result = new ObservableList<T>(true, count);

			result.BeginUpdate();

			for (int i = 1; i <= count; i++)
			{
				result.Add(create(i));
			}

			result.EndUpdate();

			return result;
		}

		/// <summary>
		/// Retrieves all the elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <typeparam name="T">Item type.</typeparam>
		/// <param name="source">Items.</param>
		/// <param name="match">The Predicate{T} delegate that defines the conditions of the elements to search for.</param>
		/// <returns>A List{T} containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty List{T}.</returns>
		public static ObservableList<T> FindAll<T>(List<T> source, Func<T, bool> match)
		{
			var result = new ObservableList<T>();

			for (int i = 0; i < source.Count; i++)
			{
				if (match(source[i]))
				{
					result.Add(source[i]);
				}
			}

			return result;
		}

		/// <summary>
		/// Get sum of the list.
		/// </summary>
		/// <param name="source">List to sum.</param>
		/// <returns>Sum.</returns>
		public static float Sum(List<float> source)
		{
			var result = 0f;

			for (int i = 0; i < source.Count; i++)
			{
				result += source[i];
			}

			return result;
		}

		/// <summary>
		/// Get or add component.
		/// </summary>
		/// <returns>Component.</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">Component type.</typeparam>
		public static T GetOrAddComponent<T>(Component obj)
			where T : Component
		{
			var component = obj.GetComponent<T>();
			if (component == null)
			{
				component = obj.gameObject.AddComponent<T>();
			}

			return component;
		}

		/// <summary>
		/// Get or add component.
		/// </summary>
		/// <returns>Component.</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">Component type.</typeparam>
		public static T GetOrAddComponent<T>(GameObject obj)
			where T : Component
		{
			var component = obj.GetComponent<T>();
			if (component == null)
			{
				component = obj.AddComponent<T>();
			}

			return component;
		}

		/// <summary>
		/// Get or add component.
		/// </summary>
		/// <param name="source">Source component.</param>
		/// <param name="target">Target component.</param>
		/// <typeparam name="T">Component type.</typeparam>
		public static void GetOrAddComponent<T>(Component source, ref T target)
			where T : Component
		{
			if ((target != null) || (source == null))
			{
				return;
			}

			target = GetOrAddComponent<T>(source);
		}

		/// <summary>
		/// Fix the instantiated object (in some cases object have wrong position, rotation and scale).
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="instance">Instance.</param>
		public static void FixInstantiated(Component source, Component instance)
		{
			FixInstantiated(source.gameObject, instance.gameObject);
		}

		/// <summary>
		/// Fix the instantiated object (in some cases object have wrong position, rotation and scale).
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="instance">Instance.</param>
		public static void FixInstantiated(GameObject source, GameObject instance)
		{
			var defaultRectTransform = source.transform as RectTransform;
			if (defaultRectTransform == null)
			{
				return;
			}

			var rectTransform = instance.transform as RectTransform;

			rectTransform.localPosition = defaultRectTransform.localPosition;
			rectTransform.localRotation = defaultRectTransform.localRotation;
			rectTransform.localScale = defaultRectTransform.localScale;
			rectTransform.anchoredPosition = defaultRectTransform.anchoredPosition;
			rectTransform.sizeDelta = defaultRectTransform.sizeDelta;
		}

		/// <summary>
		/// Finds the canvas.
		/// </summary>
		/// <returns>The canvas.</returns>
		/// <param name="currentObject">Current object.</param>
		public static Transform FindCanvas(Transform currentObject)
		{
			var canvas = currentObject.GetComponentInParent<Canvas>();
			if (canvas == null)
			{
				return null;
			}

			return canvas.transform;
		}

		/// <summary>
		/// Finds the topmost canvas.
		/// </summary>
		/// <returns>The canvas.</returns>
		/// <param name="currentObject">Current object.</param>
		public static Transform FindTopmostCanvas(Transform currentObject)
		{
			var canvases = currentObject.GetComponentsInParent<Canvas>(true);
			if (canvases.Length == 0)
			{
				return null;
			}

			return canvases[canvases.Length - 1].transform;
		}

		/// <summary>
		/// Calculates the drag position.
		/// </summary>
		/// <returns>The drag position.</returns>
		/// <param name="screenPosition">Screen position.</param>
		/// <param name="canvas">Canvas.</param>
		/// <param name="canvasRect">Canvas RectTransform.</param>
		public static Vector3 CalculateDragPosition(Vector3 screenPosition, Canvas canvas, RectTransform canvasRect)
		{
			Vector3 result;
			var canvasSize = canvasRect.sizeDelta;
			Vector2 min = Vector2.zero;
			Vector2 max = canvasSize;

			var isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
			var noCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null;
			if (isOverlay || noCamera)
			{
				result = screenPosition;
			}
			else
			{
				var ray = canvas.worldCamera.ScreenPointToRay(screenPosition);
				var plane = new Plane(canvasRect.forward, canvasRect.position);

				float distance;
				plane.Raycast(ray, out distance);

				result = canvasRect.InverseTransformPoint(ray.origin + (ray.direction * distance));

				min = -Vector2.Scale(max, canvasRect.pivot);
				max = canvasSize - min;
			}

			result.x = Mathf.Clamp(result.x, min.x, max.y);
			result.y = Mathf.Clamp(result.y, min.x, max.y);

			return result;
		}

		/// <summary>
		/// Updates the layout.
		/// </summary>
		/// <param name="layout">Layout.</param>
		[Obsolete("Use LayoutUtilities.UpdateLayout() instead.")]
		public static void UpdateLayout(LayoutGroup layout)
		{
			LayoutUtilities.UpdateLayout(layout);
		}

		/// <summary>
		/// Get top left corner position of specified RectTransform.
		/// </summary>
		/// <param name="rect">RectTransform.</param>
		/// <returns>Top left corner position.</returns>
		public static Vector2 GetTopLeftCorner(RectTransform rect)
		{
			var size = rect.rect.size;
			var pos = rect.anchoredPosition;
			var pivot = rect.pivot;

			pos.x -= size.x * pivot.x;
			pos.y += size.y * (1f - pivot.y);

			return pos;
		}

		/// <summary>
		/// Set top left corner position of specified RectTransform.
		/// </summary>
		/// <param name="rect">RectTransform.</param>
		/// <param name="position">Top left corner position.</param>
		public static void SetTopLeftCorner(RectTransform rect, Vector2 position)
		{
			var delta = position - GetTopLeftCorner(rect);
			rect.anchoredPosition += delta;
		}

		/// <summary>
		/// Suspends the coroutine execution for the given amount of seconds using unscaled time.
		/// </summary>
		/// <param name="seconds">Delay in seconds.</param>
		/// <returns>Yield instruction to wait.</returns>
		public static IEnumerator WaitForSecondsUnscaled(float seconds)
		{
			var end_time = GetTime(true) + seconds;
			while (GetTime(true) < end_time)
			{
				yield return null;
			}
		}

		/// <summary>
		/// Check how much time takes to run specified action.
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="log">Text to add to log.</param>
		public static void CheckRunTime(Action action, string log = "")
		{
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			action();

			sw.Stop();
			var ts = sw.Elapsed;

			string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:0000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
			Debug.Log("RunTime " + elapsedTime + "; " + log);
		}

		/// <summary>
		/// Determines if slider is horizontal.
		/// </summary>
		/// <returns><c>true</c> if slider is horizontal; otherwise, <c>false</c>.</returns>
		/// <param name="slider">Slider.</param>
		public static bool IsHorizontal(Slider slider)
		{
			return slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft;
		}

		/// <summary>
		/// Convert specified color to RGB hex.
		/// </summary>
		/// <returns>RGB hex.</returns>
		/// <param name="c">Color.</param>
		public static string RGB2Hex(Color32 c)
		{
			return string.Format("#{0:X2}{1:X2}{2:X2}", c.r, c.g, c.b);
		}

		/// <summary>
		/// Convert specified color to RGBA hex.
		/// </summary>
		/// <returns>RGBA hex.</returns>
		/// <param name="c">Color.</param>
		public static string RGBA2Hex(Color32 c)
		{
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
		}

		/// <summary>
		/// Converts the string representation of a number to its Byte equivalent. A return value indicates whether the conversion succeeded or failed.
		/// </summary>
		/// <returns><c>true</c> if hex was converted successfully; otherwise, <c>false</c>.</returns>
		/// <param name="hex">A string containing a number to convert.</param>
		/// <param name="result">When this method returns, contains the 8-bit unsigned integer value equivalent to the number contained in s if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or represents a number less than MinValue or greater than MaxValue. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
		public static bool TryParseHex(string hex, out byte result)
		{
			return byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result);
		}

		/// <summary>
		/// Converts the string representation of a color to its Color equivalent. A return value indicates whether the conversion succeeded or failed.
		/// </summary>
		/// <returns><c>true</c> if hex was converted successfully; otherwise, <c>false</c>.</returns>
		/// <param name="hex">A string containing a color to convert.</param>
		/// <param name="result">When this method returns, contains the color value equivalent to the color contained in hex if the conversion succeeded, or Color.black if the conversion failed. The conversion fails if the hex parameter is null or String.Empty, is not of the correct format. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
		public static bool TryHexToRGBA(string hex, out Color32 result)
		{
			result = Color.black;

			if (string.IsNullOrEmpty(hex))
			{
				return false;
			}

			var h = hex.Trim(new char[] { '#', ';' });
			byte r, g, b, a;

			if (h.Length == 8)
			{
				if (!TryParseHex(h.Substring(0, 2), out r))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(2, 2), out g))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(4, 2), out b))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(6, 2), out a))
				{
					return false;
				}
			}
			else if (h.Length == 6)
			{
				if (!TryParseHex(h.Substring(0, 2), out r))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(2, 2), out g))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(4, 2), out b))
				{
					return false;
				}

				a = 255;
			}
			else if (h.Length == 3)
			{
				if (!TryParseHex(h.Substring(0, 1), out r))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(1, 1), out g))
				{
					return false;
				}

				if (!TryParseHex(h.Substring(2, 1), out b))
				{
					return false;
				}

				r *= 17;
				g *= 17;
				b *= 17;
				a = 255;
			}
			else
			{
				return false;
			}

			result = new Color32(r, g, b, a);

			return true;
		}

		/// <summary>
		/// Is two float values is nearly equal?
		/// </summary>
		/// <param name="a">First value.</param>
		/// <param name="b">Second value.</param>
		/// <param name="epsilon">Epsilon.</param>
		/// <returns>true if two float values is nearly equal; otherwise false.</returns>
		public static bool NearlyEqual(float a, float b, float epsilon)
		{
			if (a == b)
			{
				return true;
			}

			var diff = Mathf.Abs(a - b);

			if ((a == 0) || (b == 0) || (diff < float.Epsilon))
			{
				return diff < (epsilon * float.Epsilon);
			}

			var absA = Mathf.Abs(a);
			var absB = Mathf.Abs(b);

			return (diff / (absA + absB)) < epsilon;
		}

		/// <summary>
		/// Prints the specified list to log.
		/// </summary>
		/// <typeparam name="T">Type.</typeparam>
		/// <param name="list">List.</param>
		public static void Log<T>(List<T> list)
		{
			var arr = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				arr[i] = list[i].ToString();
			}

			Debug.Log(string.Join("; ", arr));
		}

		/// <summary>
		/// Is pointer over screen?
		/// </summary>
		/// <returns>true if pointer over screen; otherwise false.</returns>
		public static bool IsPointerOverScreen()
		{
#if UNITY_EDITOR
			var screen_size = Handles.GetMainGameViewSize();
#else
			var screen_size = new Vector2(Screen.width, Screen.height);
#endif
			if ((Input.mousePosition.x <= 0)
				|| (Input.mousePosition.y <= 0)
				|| (Input.mousePosition.x >= (screen_size.x - 1))
				|| (Input.mousePosition.y >= (screen_size.y - 1)))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get graphic component from TextAdapter.
		/// </summary>
		/// <param name="adapter">Adapter.</param>
		/// <returns>Graphic component.</returns>
		public static Graphic GetGraphic(TextAdapter adapter)
		{
			return (adapter != null) ? adapter.Graphic : null;
		}

		/// <summary>
		/// Default handle for the property changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public static void DefaultPropertyHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Default handle for the changed event.
		/// </summary>
		public static void DefaultHandler()
		{
		}

		/// <summary>
		/// Copy RectTransform values.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="target">Target.</param>
		public static void CopyRectTransformValues(RectTransform source, RectTransform target)
		{
			target.anchorMin = source.anchorMin;
			target.anchorMax = source.anchorMax;
			target.pivot = source.pivot;
			target.sizeDelta = source.sizeDelta;
			target.localPosition = source.localPosition;
			target.localRotation = source.localRotation;
			target.localScale = source.localScale;
		}
	}
}