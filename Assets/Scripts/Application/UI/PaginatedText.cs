#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SecretHistories.UI {
    public class PaginatedText : NoteSphereAnimation {

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

		public void Reset()
		{
			targetPage = currentPage;
			var animOut = NoteSphereAnimation.AnimType.None;
			var animIn = NoteSphereAnimation.AnimType.MoveRight;

			TriggerAnim(animOut, animIn, SetPageToTargetPage, null);	
		}

        public void SetText(string description) {
          

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
        }

		void ShowPageNum(int page) {
			ShowPage(page - currentPage, AnimDirection.Switch);
		}

        void ShowPrevPage() {
			ShowPage(-1, AnimDirection.MoveLeft);
			SoundManager.PlaySfx("SituationWindowTextMove");
        }

       public void ShowFinalPage() {
            var offsetToLast = currentPage + Notes.Count-1;
            ShowPage(offsetToLast, AnimDirection.MoveRight); 
        }

       public void SetFinalPage()
       {
           var offsetToLast = currentPage + Notes.Count - 1;
           SetPage(offsetToLast);
       }

        void ShowNextPage() {
			ShowPage(1, AnimDirection.MoveRight);
			SoundManager.PlaySfx("SituationWindowTextMove");
        }

		void ShowPage(int offset, AnimDirection anim)
		{
			if (IsBusy())
				return;

			if (gameObject.activeInHierarchy)
			{
                targetPage = currentPage + offset;

                var animOut = (anim != AnimDirection.MoveLeft ? NoteSphereAnimation.AnimType.MoveLeft : NoteSphereAnimation.AnimType.MoveRight);
                var animIn = (anim != AnimDirection.MoveLeft ? NoteSphereAnimation.AnimType.MoveRight : NoteSphereAnimation.AnimType.MoveLeft);

                TriggerAnim(animOut, animIn, SetPageToTargetPage, null);
            }
			else
			{
				SetPage(currentPage + offset);
            }
        }

		void SetPage(int page) {

            if (Notes.Count <= 0)
            {
                NoonUtility.LogWarning("Tried to set a notes page when there are no notes");
                return;
            }

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