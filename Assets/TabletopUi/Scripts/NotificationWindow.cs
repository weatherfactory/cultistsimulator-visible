#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
	public class NotificationWindow: AnimatedNoteBase, IPointerClickHandler
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
			TriggerAnim(AnimType.None, AnimType.MoveLeft);
		}

        public void Hide()
		{
			if (gameObject.activeInHierarchy && !IsBusy()) {
				TriggerAnim(AnimType.MoveRight, AnimType.None, DoDisable);
			}
        }

        public void HideNoDestroy()
		{
			if (gameObject.activeInHierarchy && !IsBusy()) {
				TriggerAnim(AnimType.MoveRight, AnimType.None, StopAllCoroutines);
			}
        }

		protected void DoDisable()
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
