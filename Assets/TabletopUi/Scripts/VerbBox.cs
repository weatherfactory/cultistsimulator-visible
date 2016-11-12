using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class VerbBox : DraggableToken {

        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI text; // Currently can be above boxes. Ideally should always be behind boxes - see shadow for solution?
        [SerializeField] Image countdownBar;
        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] GameObject selectedMarker;
        private Verb _verb;


        [HideInInspector] public SituationWindow detailsWindow;
        public string verbId { get {
            return _verb == null ? null : _verb.Id;
        } }

        // Question how much of that we retain in the DisplayObject or in the Verb.
        // For convenienence I think it makes sense to keep it in the verb/situation/recipe and either
        // referencing them here for easy access or having the verb constantly push
        // updates to a set of duplicate fields.
        public bool isBusy { private set; get; }
        private float timeRemaining = 0f;
        private int numCompletions = 0; // Stands for the amount of completed cycles.

        public void SetVerb(string id) {
            var verb = Registry.Compendium.GetVerbById(id);

            if (verb != null)
                SetVerb(verb);
        }

        public void SetVerb(Verb verb)
        {
            _verb = verb;
            name = "Verb_" + verbId;

            if (verb == null)
                return;

            DisplayName(verb);
            DisplayIcon(verb);
            SetSelected(false);
            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
        }

        private void DisplayName(Verb v) {
            text.text = v.Label;
        }

        private void DisplayIcon(Verb v) {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(v.Id);
            artwork.sprite = sprite;
        }

        public void SetSelected(bool isSelected) {
            selectedMarker.gameObject.SetActive(isSelected);
        }

        public Sprite GetSprite() {
            return artwork.sprite;
        }

        public void StartTimer() {
            if (isBusy)
                return;
		
            StopAllCoroutines();
            StartCoroutine(DoTimer(10f));
        }

        IEnumerator DoTimer(float duration) {
            isBusy = true;
            timeRemaining = duration;
            countdownBar.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);

            while (timeRemaining > 0f) {
                timeRemaining -= Time.deltaTime;
                countdownBar.fillAmount = 1f - (timeRemaining / duration);
                countdownText.text = timeRemaining.ToString("0.0") + "s";
                yield return null;
            }

            numCompletions++;

            countdownBar.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
            timeRemaining = 0f;
            isBusy = false;
        }

        // Interaction

       
    }
}
