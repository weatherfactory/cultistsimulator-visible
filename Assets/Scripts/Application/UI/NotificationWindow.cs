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

        public void SetDuration(float duration)
        {
            Invoke("Hide", duration);
        }

        public void SetDetails(string title, string description)
        {
            _titleTxt.text = title;
            _descriptionTxt.text = description;
        }

		public void Show() {
			gameObject.SetActive(true);

			// Make the anim move out, then show the content, then move in again
			TriggerAnimation(new NavigationArgs(0,NavigationAnimationDirection.None,NavigationAnimationDirection.MoveLeft), null,null);
		}

        public void Hide()
		{
			if (gameObject.activeInHierarchy && !IsBusy()) {
                TriggerAnimation(new NavigationArgs(0, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.None), null, Cleanup);
            }
        }

  //      public void HideNoDestroy()
		//{
		//	if (gameObject.activeInHierarchy && !IsBusy()) {
  //              TriggerAnimation(AnimType.MoveRight, AnimType.None, StopAllCoroutines);
		//	}
  //      }

        protected void E(NavigationArgs args)
        {

        }

		protected void Cleanup(NavigationArgs args)
		{
			StopAllCoroutines();
			Destroy(gameObject);
		}

        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
