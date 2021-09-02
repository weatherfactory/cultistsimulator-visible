#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI
{
	public class NotificationWindow: NavigationAnimation, IPointerClickHandler
    {
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI _titleTxt;
        [SerializeField] TextMeshProUGUI _descriptionTxt;
        [SerializeField] TextMeshProUGUI _additionalText;
        /// <summary>
        /// Permanent notification windows
        /// - are not hidden on click - they must be explicitly closed
        /// - are disabled, not destroyed, when hidden
        /// </summary>
        [SerializeField] private bool Permanent;

        public string Title
        {
	        get
	        {
		        return _titleTxt.text;
	        }
        }
        
        public string Description
        {
	        get
	        {
		        return _descriptionTxt.text;
	        }
        }

        public string AdditionalText
        {
            get
            {
                return _additionalText.text;
            }
        }

        public void SetDuration(float duration)
        {
            Invoke("Hide", duration);
        }

        public void SetDetails(string title, string description)
        {
            SetDetails(title,description,string.Empty);
        }
        public void SetDetails(string title, string description,string additionalText)
        {
            _titleTxt.text = title;
            _descriptionTxt.text = description;
            _additionalText.text = additionalText;
        }

        public void Show() {
			gameObject.SetActive(true);

			// Make the anim move out, then show the content, then move in again
			TriggerAnimation(new NavigationArgs(0,NavigationAnimationDirection.None,NavigationAnimationDirection.MoveLeft));
		}

        public void Hide()
		{
			if (gameObject.activeInHierarchy && !IsBusy())
            {
                var args = new NavigationArgs(0, NavigationAnimationDirection.MoveRight,
                    NavigationAnimationDirection.None);
                args.OnEnd = Cleanup;

                TriggerAnimation(args);

            }
        }



		protected void Cleanup(NavigationArgs args)
		{
			StopAllCoroutines();
            if(!Permanent)
			    Destroy(gameObject);
            else
                gameObject.SetActive(false);
		}

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!Permanent)
                Hide();
        }
    }
}
