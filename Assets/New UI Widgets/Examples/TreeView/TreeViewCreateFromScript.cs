namespace UIWidgets.Examples
{
	using System.Collections.Generic;
	using EasyLayoutNS;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Vreate TreeView from script.
	/// </summary>
	public class TreeViewCreateFromScript : MonoBehaviour
	{
		#region Create TreeView from prefab

		/// <summary>
		/// The TreeView parent.
		/// </summary>
		public RectTransform TreeViewParent;

		/// <summary>
		/// The TreeView prefab.
		/// </summary>
		public TreeView TreeViewPrefab;

		/// <summary>
		/// Creates the TreeView from prefab.
		/// </summary>
		public void CreateTreeViewFromPrefab()
		{
			var tree_view = Compatibility.Instantiate(TreeViewPrefab);
			tree_view.transform.SetParent(TreeViewParent, false);

			// do something with tree_view
		}
		#endregion

		#region Create TreeView with script

		/// <summary>
		/// The tree root.
		/// </summary>
		public RectTransform TreeRoot;

		/// <summary>
		/// The main background.
		/// </summary>
		public Sprite MainBackground;

		/// <summary>
		/// The default color.
		/// </summary>
		public Color ColorDefault;

		/// <summary>
		/// The highlighted color.
		/// </summary>
		public Color ColorHighlighted;

		/// <summary>
		/// The selected color.
		/// </summary>
		public Color ColorSelected;

		/// <summary>
		/// The default background color.
		/// </summary>
		public Color BGColorDefault;

		/// <summary>
		/// The highlighted background color.
		/// </summary>
		public Color BGColorHighlighted;

		/// <summary>
		/// The selected background color.
		/// </summary>
		public Color BGColorSelected;

		/// <summary>
		/// The viewport background.
		/// </summary>
		public Sprite ViewportBackground;

		/// <summary>
		/// The scrollbar background.
		/// </summary>
		public Sprite ScrollbarBackground;

		/// <summary>
		/// The scrollbar handle.
		/// </summary>
		public Sprite ScrollbarHandle;

		/// <summary>
		/// The item background.
		/// </summary>
		public Sprite ItemBackground;

		/// <summary>
		/// The item background color.
		/// </summary>
		public Color ItemBackgroundColor;

		/// <summary>
		/// The item toggle.
		/// </summary>
		public Sprite ItemToggle;

		/// <summary>
		/// The text font.
		/// </summary>
		public Font TextFont;

		static RectTransform GetRectTrasform(GameObject go)
		{
			var rt = go.GetComponent<RectTransform>();
			if (rt != null)
			{
				return rt;
			}

			return go.AddComponent<RectTransform>();
		}

		/// <summary>
		/// Tests the TreeView creation.
		/// </summary>
		public void TestCreateTreeView()
		{
			var tree = CreateTreeView(TreeRoot);

			var config = new List<int>() { 10, 4, 5, 5, };
			tree.Nodes = TestTreeView.GenerateTreeNodes(config, isExpanded: true);
		}

		/// <summary>
		/// Creates the TreeView.
		/// </summary>
		/// <returns>The tree view.</returns>
		/// <param name="root">Root.</param>
		public TreeView CreateTreeView(RectTransform root)
		{
			var tree_go = new GameObject("TreeView");
			tree_go.transform.SetParent(root, false);

			var rt = GetRectTrasform(tree_go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = new Vector2(520, 540);
			rt.pivot = new Vector2(0, 1);

			var container = CreateContainer(rt);
			var default_item = CreateDefaultItem(container);
			var scroll_view = CreateScrollView(rt, container);

			var image = tree_go.AddComponent<Image>();
			image.sprite = MainBackground;
			image.type = Image.Type.Sliced;

			var tree_view = tree_go.AddComponent<TreeView>();
			tree_view.DefaultItem = default_item;
			tree_view.Container = container;
			tree_view.ScrollRect = scroll_view;

			tree_view.DefaultColor = ColorDefault;
			tree_view.HighlightedColor = ColorHighlighted;
			tree_view.SelectedColor = ColorSelected;

			tree_view.DefaultBackgroundColor = BGColorDefault;
			tree_view.HighlightedBackgroundColor = BGColorHighlighted;
			tree_view.SelectedBackgroundColor = BGColorSelected;

			tree_view.Init();

			return tree_view;
		}

		ScrollRect CreateScrollView(RectTransform root, RectTransform content)
		{
			var scroll_view_go = new GameObject("ScrollRect");
			scroll_view_go.transform.SetParent(root, false);

			var scroll_view_rt = GetRectTrasform(scroll_view_go);
			scroll_view_rt.anchorMin = new Vector2(0, 0);
			scroll_view_rt.anchorMax = new Vector2(1, 1);
			scroll_view_rt.sizeDelta = new Vector2(-21, -21);
			scroll_view_rt.anchoredPosition = new Vector2(-9.5f, 9.5f);
			scroll_view_rt.pivot = new Vector2(0.5f, 0.5f);

			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var viewport = CreateViewport(scroll_view_rt, content);
			#else
			CreateViewport(scroll_view_rt, content);
			#endif

			var scrollbar = CreateScrollBar(scroll_view_rt);

			var scroll_rect = scroll_view_go.AddComponent<ScrollRect>();
			scroll_rect.horizontal = false;
			scroll_rect.vertical = true;
			scroll_rect.content = content;
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			scroll_rect.viewport = viewport;
			#endif
			scroll_rect.verticalScrollbar = scrollbar;

			return scroll_rect;
		}

		RectTransform CreateViewport(RectTransform root, RectTransform content)
		{
			var viewport_go = new GameObject("Viewport");
			viewport_go.transform.SetParent(root, false);
			var viewport_rt = GetRectTrasform(viewport_go);
			viewport_rt.anchorMin = new Vector2(0, 0);
			viewport_rt.anchorMax = new Vector2(1, 1);
			viewport_rt.sizeDelta = new Vector2(0, 0);
			viewport_rt.anchoredPosition = new Vector2(0, 0);
			viewport_rt.pivot = new Vector2(0.5f, 0.5f);

			content.SetParent(viewport_rt, false);
			content.anchorMin = new Vector2(0, 1);
			content.anchorMax = new Vector2(1, 1);
			content.sizeDelta = new Vector2(0, 0);
			content.anchoredPosition = new Vector2(0, 0);
			content.pivot = new Vector2(0, 1);

			var image = viewport_go.AddComponent<Image>();
			image.sprite = ViewportBackground;
			image.type = Image.Type.Sliced;

			var mask = viewport_go.AddComponent<Mask>();
			mask.showMaskGraphic = false;

			return viewport_rt;
		}

		Scrollbar CreateScrollBar(RectTransform root)
		{
			var scrollbar_go = new GameObject("VScrollbar");
			scrollbar_go.transform.SetParent(root, false);
			var scrollbar_rt = GetRectTrasform(scrollbar_go);
			scrollbar_rt.anchorMin = new Vector2(1, 0);
			scrollbar_rt.anchorMax = new Vector2(1, 1);
			scrollbar_rt.sizeDelta = new Vector2(12, -3);
			scrollbar_rt.anchoredPosition = new Vector2(4, 0);
			scrollbar_rt.pivot = new Vector2(0, 0);

			var scrollbar = scrollbar_go.AddComponent<Scrollbar>();

			CreateScrollBarBackground(scrollbar_rt);

			var handle = CreateScrollBarHandle(scrollbar_rt);

			scrollbar.transition = Selectable.Transition.ColorTint;
			scrollbar.targetGraphic = handle.GetComponent<Image>();
			scrollbar.handleRect = handle;
			scrollbar.direction = Scrollbar.Direction.BottomToTop;

			return scrollbar;
		}

		void CreateScrollBarBackground(RectTransform root)
		{
			var bg_go = new GameObject("Background");
			bg_go.transform.SetParent(root, false);
			var bg_rt = GetRectTrasform(bg_go);
			bg_rt.anchorMin = new Vector2(0.5f, 0);
			bg_rt.anchorMax = new Vector2(0.5f, 1);
			bg_rt.sizeDelta = new Vector2(2, -6);
			bg_rt.anchoredPosition = new Vector2(0, 0);
			bg_rt.pivot = new Vector2(0.5f, 0.5f);

			var image = bg_go.AddComponent<Image>();
			image.sprite = ScrollbarBackground;
		}

		RectTransform CreateScrollBarHandle(RectTransform root)
		{
			var sliding_area_go = new GameObject("Sliding area");
			sliding_area_go.transform.SetParent(root, false);
			var sliding_area_rt = GetRectTrasform(sliding_area_go);
			sliding_area_rt.anchorMin = new Vector2(0, 0);
			sliding_area_rt.anchorMax = new Vector2(1, 1);
			sliding_area_rt.anchoredPosition = new Vector2(0, 0);
			sliding_area_rt.sizeDelta = new Vector2(-20, -20);
			sliding_area_rt.pivot = new Vector2(0.5f, 0.5f);

			var hangle_go = new GameObject("Handle");
			hangle_go.transform.SetParent(sliding_area_rt, false);
			var handle_rt = GetRectTrasform(hangle_go);
			handle_rt.anchorMin = new Vector2(0, 0);
			handle_rt.anchorMax = new Vector2(1, 1);
			handle_rt.sizeDelta = new Vector2(20, 20);
			handle_rt.anchoredPosition = new Vector2(0, 0);
			handle_rt.pivot = new Vector2(0.5f, 0.5f);

			var image = hangle_go.AddComponent<Image>();
			image.sprite = ScrollbarHandle;
			image.type = Image.Type.Sliced;

			return handle_rt;
		}

		static RectTransform CreateContainer(RectTransform root)
		{
			var go = new GameObject("List");
			go.transform.SetParent(root, false);

			var layout = go.AddComponent<EasyLayout>();
			layout.LayoutType = LayoutTypes.Grid;
			layout.GridConstraint = GridConstraints.FixedColumnCount;
			layout.GridConstraintCount = 1;
			layout.Spacing = new Vector2(2, 2);
			layout.Margin = new Vector2(5, 5);
			layout.ChildrenWidth = ChildrenSize.SetMaxFromPreferred;

			var fitter = go.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			return GetRectTrasform(go);
		}

		TreeViewComponent CreateDefaultItem(RectTransform root)
		{
			var go = new GameObject("DefaultItem");
			go.transform.SetParent(root, false);

			var rt = GetRectTrasform(go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(1, 1);
			rt.sizeDelta = new Vector2(-10, 36);
			rt.anchoredPosition = new Vector2(0, -23);
			rt.pivot = new Vector2(0.5f, 0.5f);

			var image = go.AddComponent<Image>();
			image.sprite = ItemBackground;
			image.color = ItemBackgroundColor;
			image.type = Image.Type.Sliced;

			// add TreeViewComponent
			var tree = go.AddComponent<TreeViewComponent>();
			tree.Indentation = CreateDefaultItemIndendation(rt);
			tree.Toggle = CreateDefaultItemToggle(rt);
			tree.Icon = CreateDefaultItemIcon(rt);
			tree.TextAdapter = CreateDefaultItemText(rt);
			tree.OnNodeExpand = NodeToggle.Rotate;
			tree.PaddingPerLevel = 30;

			// add layout element
			var le = go.AddComponent<LayoutElement>();
			le.minWidth = 489;
			#if UNITY_2017_1_OR_NEWER
			le.layoutPriority = 1;
			#endif

			var layout = go.AddComponent<EasyLayout>();
			layout.MainAxis = Axis.Vertical;
			layout.LayoutType = LayoutTypes.Compact;
			layout.Spacing = new Vector2(5, 5);
			layout.Symmetric = false;
			layout.MarginTop = 7;
			layout.MarginBottom = 5;
			layout.MarginLeft = 5;
			layout.MarginRight = 10;
			layout.ChildrenWidth = ChildrenSize.SetPreferred;
			layout.SkipInactive = false;

			return tree;
		}

		static LayoutElement CreateDefaultItemIndendation(RectTransform root)
		{
			var go = new GameObject("Indentation");
			go.transform.SetParent(root, false);

			var rt = GetRectTrasform(go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(1, 1);
			rt.pivot = new Vector2(0.5f, 0.5f);

			var le = go.AddComponent<LayoutElement>();
			le.preferredWidth = 0;
			#if UNITY_2017_1_OR_NEWER
			le.layoutPriority = 1;
			#endif

			return le;
		}

		TreeNodeToggle CreateDefaultItemToggle(RectTransform root)
		{
			var go = new GameObject("Toggle");
			go.transform.SetParent(root, false);

			var rt = GetRectTrasform(go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.sizeDelta = new Vector2(20, 19);
			rt.pivot = new Vector2(0.5f, 0.5f);

			var image = go.AddComponent<Image>();
			image.sprite = ItemToggle;

			return go.AddComponent<TreeNodeToggle>();
		}

		static Image CreateDefaultItemIcon(RectTransform root)
		{
			var go = new GameObject("Icon");
			go.transform.SetParent(root, false);

			var rt = GetRectTrasform(go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.sizeDelta = new Vector2(0, 20);
			rt.pivot = new Vector2(0.5f, 0.5f);

			var image = go.AddComponent<Image>();

			return image;
		}

		TextAdapter CreateDefaultItemText(RectTransform root)
		{
			var go = new GameObject("Text");
			go.transform.SetParent(root, false);

			var rt = GetRectTrasform(go);
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.sizeDelta = new Vector2(0, 21);
			rt.pivot = new Vector2(0.5f, 0.5f);

			var text = go.AddComponent<Text>();
			text.font = TextFont;
			text.fontSize = 18;
			text.alignment = TextAnchor.MiddleLeft;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;

			return go.AddComponent<TextAdapter>();
		}
		#endregion
	}
}