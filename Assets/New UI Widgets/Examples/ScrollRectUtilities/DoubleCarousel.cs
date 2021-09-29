namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Double carousel.
	/// </summary>
	public class DoubleCarousel : MonoBehaviour
	{
		/// <summary>
		/// Paginator with direct scroll.
		/// </summary>
		[SerializeField]
		protected ScrollRectPaginator DirectPaginator;

		/// <summary>
		/// ScrollRect to reverse scroll.
		/// </summary>
		[SerializeField]
		protected ScrollRect ReverseScrollRect;

		/// <summary>
		/// Images scale.
		/// </summary>
		[SerializeField]
		protected float Scale = 1.5f;

		RectTransform ReverseContent;

		/// <summary>
		/// Process the start event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0603:Delegate allocation from a method group", Justification = "Required.")]
		protected void Start()
		{
			ReverseContent = ReverseScrollRect.content;

			// duplicate first and last slides
			var first = ReverseContent.GetChild(0);
			var last = ReverseContent.GetChild(ReverseContent.childCount - 1);
			Instantiate(first, ReverseContent, true);
			Instantiate(last, ReverseContent, true).SetAsFirstSibling();

			// init
			DirectPaginator.OnMovement.AddListener(UpdateReverse);
			DirectPaginator.Init();
			UpdateReverse(DirectPaginator.CurrentPage, 0f);
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0603:Delegate allocation from a method group", Justification = "Required.")]
		protected void OnDestroy()
		{
			if (DirectPaginator != null)
			{
				DirectPaginator.OnMovement.RemoveListener(UpdateReverse);
			}
		}

		void UpdateReverse(int page, float ratio)
		{
			// scroll in the reverse direction
			var position = DirectPaginator.GetContentSize() - DirectPaginator.GetPosition();
			ReverseContent.anchoredPosition = DirectPaginator.IsHorizontal()
				? new Vector2(-position, ReverseContent.anchoredPosition.y)
				: new Vector2(ReverseContent.anchoredPosition.x, position);

			// scale and fade in/out image
			var clamped_page = ClampDirectPage(page);
			var reverse_page = (page == -1)
				? ReverseContent.childCount - 1
				: ClampReversePage(ReverseContent.childCount - clamped_page - 2);
			ScaleAndFade(reverse_page, ratio);
			ScaleAndFade(ClampReversePage(reverse_page - 1), 1f - ratio);
		}

		int ClampDirectPage(int page)
		{
			if (page < 0)
			{
				page += DirectPaginator.Pages;
			}

			if (page >= DirectPaginator.Pages)
			{
				page -= DirectPaginator.Pages;
			}

			return page;
		}

		int ClampReversePage(int page)
		{
			if (page < 0)
			{
				page += ReverseContent.childCount;
			}

			if (page >= ReverseContent.childCount)
			{
				page -= ReverseContent.childCount;
			}

			return page;
		}

		void ScaleAndFade(int index, float ratio)
		{
			var page = ReverseContent.GetChild(index);
			var scale = Mathf.Lerp(1f, Scale, ratio);
			page.localScale = new Vector3(scale, scale, scale);

			var graphic = page.GetComponent<Graphic>();
			var color = graphic.color;
			color.a = 1f - ratio;
			graphic.color = color;
		}
	}
}