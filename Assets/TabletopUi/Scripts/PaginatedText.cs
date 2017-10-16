using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.CS.TabletopUI {
    public class PaginatedText : MonoBehaviour {

        enum AnimDirection { MoveRight, MoveLeft, Switch }

        [SerializeField] Animation anim;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Button prevPage;
        [SerializeField] Button nextPage;

        [SerializeField] List<string> texts = new List<string>();
        bool isBusy;
        int currentPage = 0;

        void OnEnable() {
            prevPage.onClick.AddListener(ShowPrevPage);
            nextPage.onClick.AddListener(ShowNextPage);
            SetPage(0);
        }

        void OnDisable() {
            prevPage.onClick.RemoveListener(ShowPrevPage);
            nextPage.onClick.RemoveListener(ShowNextPage);
			isBusy = false;
        }

        public void SetText(string text) {
            texts.Clear();
            texts.Add(text);
			ShowPageNum(0);
        }

		public void SetText(List<string> text) {
			texts = text;
			ShowPageNum(text.Count - 1);
		}

        public void AddText(string text) {
            texts.Add(text);
			ShowNextPage();
        }

		void ShowPageNum(int page) {
			ShowPage(page - currentPage, AnimDirection.Switch);
		}

        void ShowPrevPage() {
			ShowPage(-1, AnimDirection.MoveLeft);
        }

        void ShowNextPage() {
			ShowPage(1, AnimDirection.MoveRight);
        }

		void ShowPage(int offset, AnimDirection anim) {
			if (isBusy)
				return;

			if (gameObject.activeInHierarchy)
				StartCoroutine(DoAnim(anim, offset));
			else
				SetPage(currentPage + offset);
		}

		void SetPage(int page) {
			if (page + 1 >= texts.Count) 
				currentPage = texts.Count - 1;
			else if (page <= 0)
				currentPage = 0;
			else
				currentPage = page;

			text.text = texts[currentPage];
			prevPage.interactable = currentPage > 0;
			nextPage.interactable = currentPage + 1 < texts.Count;
		}


        IEnumerator DoAnim(AnimDirection direction, int offset) {
            isBusy = true;
            string clipName;
            clipName = GetOutClip(direction);

            if (clipName != null)
                anim.Play(clipName);

            while (anim.isPlaying)
                yield return null;

            SetPage(currentPage + offset);
            clipName = GetInClip(direction);

            if (clipName != null)
                anim.Play(clipName);

            isBusy = false;
        }
        string GetOutClip(AnimDirection direction) {
            if (direction == AnimDirection.MoveLeft)
                return "situation-note-move-out-r";
            else if (direction == AnimDirection.MoveRight)
				return "situation-note-move-out-l";
			else if (direction == AnimDirection.Switch)
				return "situation-note-move-out-l";
            else
                return null;
        }

        string GetInClip(AnimDirection direction) {
            if (direction == AnimDirection.MoveLeft)
                return "situation-note-move-in-l";
            else if (direction == AnimDirection.MoveRight)
				return "situation-note-move-in-r";
			else if (direction == AnimDirection.Switch)
				return "situation-note-move-in-l";
            else
                return null;
        }
    }
}