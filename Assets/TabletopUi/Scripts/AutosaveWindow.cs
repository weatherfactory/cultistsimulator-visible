#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
	public class AutosaveWindow: MonoBehaviour
    {
	    protected delegate void AnimResponse();
		protected enum AnimType { None, FadeIn, FadeOut }

		[SerializeField] Animation anim;

	    private bool isBusy;

        public void SetDuration(float duration)
        {
            Invoke("Hide", duration);
        }

		public void Show()
		{
			gameObject.SetActive(true);

			// Make the anim move out, then show the content, then move in again
			TriggerAnim(AnimType.None, AnimType.FadeIn);
		}

        public void Hide()
		{
			if (gameObject.activeInHierarchy && !isBusy)
			{
				TriggerAnim(AnimType.FadeOut, AnimType.None, DoDisable);
			}
        }

		protected void DoDisable()
		{
			StopAllCoroutines();
			gameObject.SetActive(false);
		}

		protected void TriggerAnim(AnimType animOut, AnimType animIn, AnimResponse onOutDone = null, AnimResponse onInDone = null)
		{
			StartCoroutine(DoAnim(animOut, animIn, onOutDone, onInDone));
		}

		private IEnumerator DoAnim(AnimType animOut, AnimType animIn, AnimResponse onOutDone, AnimResponse onInDone)
		{
			string clipName;
			isBusy = true;

			clipName = GetClip(animOut);
			if (clipName != null)
			{
				anim.Play(clipName);

				while (anim.isPlaying)
					yield return null;
			}

			if (onOutDone != null)
				onOutDone();

			clipName = GetClip(animIn);
			if (clipName != null)
			{
				anim.Play(clipName);

				while (anim.isPlaying)
					yield return null;
			}

			if (onInDone != null)
				onInDone();

			isBusy = false;
		}

		private string GetClip(AnimType type)
		{
			if (type == AnimType.FadeIn)
				return "autosave-show";
			else if (type == AnimType.FadeOut)
				return "autosave-hide";
			else
				return null;
		}
    }
}
