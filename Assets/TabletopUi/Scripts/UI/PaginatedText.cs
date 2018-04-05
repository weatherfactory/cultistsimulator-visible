#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.CS.TabletopUI {
    public class PaginatedText : AnimatedNoteBase {

        enum AnimDirection { MoveRight, MoveLeft, Switch }

        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Button prevPage;
        [SerializeField] Button nextPage;

        [SerializeField] List<ISituationNote> Notes = new List<ISituationNote>();
        int currentPage = 0;
        int targetPage = -1;

       public List<ISituationNote> GetCurrentTexts()
        {
            return Notes;
        }

        void OnEnable() {
            prevPage.onClick.AddListener(ShowPrevPage);
            nextPage.onClick.AddListener(ShowNextPage);

            if (Notes.Count > 0)
                SetPage(Notes.Count-1);
        }

        protected override void OnDisable() {
			base.OnDisable();
            prevPage.onClick.RemoveListener(ShowPrevPage);
            nextPage.onClick.RemoveListener(ShowNextPage);
        }

        public void SetText(string description) {
            //we often add a . to indicate that the description is intentionally empty.
            //if we do that, or if it's a mistaken empty string, just go back.
            if (description == "." || description == string.Empty)
                return;

            if (Notes.Count == 1 && Notes[0].Description == description)
                return;

            var newNote=new SituationNote(description);

            Notes.Clear();
            Notes.Add(newNote);
			ShowPageNum(0); //Ee've just added a new text stack, not added so we show the first page. 
            // This gets us a different show anim out to left, in from left. instead of directional
        }

        public void SetText(List<ISituationNote> setNotes) {
			Notes = setNotes;
			ShowPageNum(setNotes.Count - 1);
		}

        public void AddText(string description)
        {
            //we often add a . to indicate that the description is intentionally empty.
            //if we do that, or if it's a mistaken empty string, just go back.
            if (description == "." || description == string.Empty)
                return;

            if (Notes.Count > 0 && Notes[Notes.Count - 1].Description == description)
                return;
            var newNote = new SituationNote(description);

            Notes.Add(newNote);
            ShowFinalPage(); //assuming that we always want to show the last page, if a note has been added.
            //This includes adding notes on reloading.
        }

		void ShowPageNum(int page) {
			ShowPage(page - currentPage, AnimDirection.Switch);
		}

        void ShowPrevPage() {
			ShowPage(-1, AnimDirection.MoveLeft);
        }

        void ShowFinalPage() {
            var offsetToLast = currentPage + Notes.Count-1;
            ShowPage(offsetToLast, AnimDirection.MoveRight); 
        }

        void ShowNextPage() {
			ShowPage(1, AnimDirection.MoveRight);
        }

		void ShowPage(int offset, AnimDirection anim) {
			if (IsBusy())
				return;

			if (gameObject.activeInHierarchy) {
                targetPage = currentPage + offset;

                var animOut = (anim != AnimDirection.MoveLeft ? AnimatedNoteBase.AnimType.MoveLeft : AnimatedNoteBase.AnimType.MoveRight);
                var animIn = (anim != AnimDirection.MoveLeft ? AnimatedNoteBase.AnimType.MoveRight : AnimatedNoteBase.AnimType.MoveLeft);

                TriggerAnim(animOut, animIn, SetPageToTargetPage, null);
            }
			else { 
				SetPage(currentPage + offset);
            }
        }

		void SetPage(int page) {
			if (page + 1 > Notes.Count) 
				currentPage = Notes.Count - 1;
			else if (page <= 0)
				currentPage = 0;
			else
				currentPage = page;

			text.text = Notes[currentPage].Description;
			prevPage.interactable = currentPage > 0;
			nextPage.interactable = currentPage + 1 < Notes.Count;
		}

        void SetPageToTargetPage() {
            if (targetPage == -1)
                return;

            SetPage(targetPage);
            targetPage = -1;
        }

    }
}