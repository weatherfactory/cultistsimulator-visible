namespace UIWidgets.Examples
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// TreeViewSample component multiple.
	/// </summary>
	public class TreeViewSampleComponentMultiple : TreeViewSampleComponent
	{
		/// <summary>
		/// Continent template.
		/// </summary>
		[SerializeField]
		protected TreeViewSampleComponentContinent ContinentTemplate;

		/// <summary>
		/// Country template.
		/// </summary>
		[SerializeField]
		protected TreeViewSampleComponentCountry CountryTemplate;

		/// <summary>
		/// Items component parent.
		/// </summary>
		[SerializeField]
		protected Transform ComponentParent;

		ITreeViewSampleMultipleComponent CurrentComponent;

		/// <summary>
		/// Init templates.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			ContinentTemplate.gameObject.SetActive(false);
			CountryTemplate.gameObject.SetActive(false);
		}

		/// <summary>
		/// Gets the template.
		/// </summary>
		/// <returns>The template.</returns>
		/// <param name="type">Type.</param>
		protected virtual ITreeViewSampleMultipleComponent GetTemplate(Type type)
		{
			if (type == typeof(TreeViewSampleItemContinent))
			{
				return ContinentTemplate;
			}

			if (type == typeof(TreeViewSampleItemCountry))
			{
				return CountryTemplate;
			}

			return null;
		}

		/// <summary>
		/// Update view.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="ev">Event.</param>
		protected override void UpdateView(object sender = null, PropertyChangedEventArgs ev = null)
		{
			MovedToCache();

			CurrentComponent = GetTemplate(Item.GetType()).IInstance(ComponentParent);
			CurrentComponent.SetData(Item);
		}

		/// <summary>
		/// Graphics coloring.
		/// </summary>
		/// <param name="foregroundColor">Foreground color.</param>
		/// <param name="backgroundColor">Background color.</param>
		/// <param name="fadeDuration">Fade duration.</param>
		public override void GraphicsColoring(Color foregroundColor, Color backgroundColor, float fadeDuration = 0f)
		{
			base.GraphicsColoring(foregroundColor, backgroundColor, fadeDuration);

			if (CurrentComponent != null)
			{
				CurrentComponent.GraphicsColoring(foregroundColor, backgroundColor, fadeDuration);
			}
		}

		/// <summary>
		/// Called when item moved to cache, you can use it free used resources.
		/// </summary>
		public override void MovedToCache()
		{
			if (CurrentComponent != null)
			{
				CurrentComponent.Free(ContinentTemplate.transform.parent);
				CurrentComponent = null;
			}
		}
	}
}