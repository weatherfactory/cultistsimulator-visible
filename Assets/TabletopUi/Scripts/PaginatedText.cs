using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.CS.TabletopUI {
    public class PaginatedText : MonoBehaviour {

        enum AnimDirection { MoveRight, MoveLeft }

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
        }

        public void SetText(string text) {
            texts.Clear();
            texts.Add(text);
            SetPage(0);
        }

        public void AddText(string text) {
            texts.Add(text);
        }

        void ShowPrevPage() {
            if (isBusy)
                return;

            StartCoroutine(DoAnim(AnimDirection.MoveLeft, -1));
        }

        void ShowNextPage() {
            if (isBusy)
                return;

            StartCoroutine(DoAnim(AnimDirection.MoveRight, 1));
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

        string GetOutClip(AnimDirection direction) {
            if (direction == AnimDirection.MoveLeft)
                return "situation-note-move-out-r";
            else if (direction == AnimDirection.MoveRight)
                return "situation-note-move-out-l";
            else
                return null;
        }

        string GetInClip(AnimDirection direction) {
            if (direction == AnimDirection.MoveLeft)
                return "situation-note-move-in-l";
            else if (direction == AnimDirection.MoveRight)
                return "situation-note-move-in-r";
            else
                return null;
        }
    }
}