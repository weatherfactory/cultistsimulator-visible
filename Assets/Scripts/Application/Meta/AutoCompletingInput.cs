using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{


    public class AutoCompletingInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private AutoCompletionBox autoCompletionBox;
        [SerializeField] private VerticalLayoutGroup autoCompletionSuggestions;
        [SerializeField] public Transform AutoCompletionSuggestionPrefab;
        public bool MakeElementSuggestions;
        public bool MakeRecipeSuggestions;
        public bool MakeEndingSuggestions;


        private const int MaxAutoCompletionSuggestions = 50;

        // Indicates the last selected auto-completion suggestion
        // -1 means no previous suggestion was selected
        private int currentAutoCompletionSuggestion = -1;
        private string lastAutocompleteCheckedTextValue;
        public string text
        {
            get => input.text;
            set => input.text = value;
        }

        public void Awake()
        {
            autoCompletionBox.gameObject.SetActive(false);
            
        }

        public void Update()
        {
            ProcessInput();

            if (lastAutocompleteCheckedTextValue == text)
                return;

            lastAutocompleteCheckedTextValue = text;
            AttemptAutoCompletion(text);
        }

        public bool ProcessInput()
        {
            if (!autoCompletionBox.isActiveAndEnabled)
                return false;

            // If the user has right-clicked, close the suggestions box
            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
            {
                AttemptAutoCompletion(null);
                return true;
            }

            // Only process the rest when the main input field is open
            if (!input.isFocused)
                return false;

 
            List<AutoCompletionSuggestion> suggestions = new List<AutoCompletionSuggestion>();
            autoCompletionSuggestions.GetComponentsInChildren(suggestions);

            if (suggestions.Count == 0)
                return false;

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                return AutoCompleteWithFirstSuggestion(suggestions);
            }

            // Check if the user is navigating suggestions with the arrow keys
            if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                return MoveToSuggestionBasedOnArrowKey(suggestions);
            }




            return false;
        }

        private bool MoveToSuggestionBasedOnArrowKey(List<AutoCompletionSuggestion> suggestions)
        {
            // Get the next suggestion based on what was previously used
            if (currentAutoCompletionSuggestion < 0)
                currentAutoCompletionSuggestion = 0;
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                currentAutoCompletionSuggestion++;
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                currentAutoCompletionSuggestion--;

            // Fold back to beginning and end of the suggestions if we overflow
            if (currentAutoCompletionSuggestion >= suggestions.Count)
                currentAutoCompletionSuggestion = 0;
            else if (currentAutoCompletionSuggestion < 0)
                currentAutoCompletionSuggestion = suggestions.Count - 1;

            SetInput(suggestions[currentAutoCompletionSuggestion].GetText());
            input.MoveTextEnd(false);
            return true;
        }

        private bool AutoCompleteWithFirstSuggestion(List<AutoCompletionSuggestion> suggestions)
        {
            currentAutoCompletionSuggestion = 0;
            AutoCompletionSuggestion suggestion = suggestions.First();
            SetInput(suggestion.GetText());
            input.MoveTextEnd(false);
            return true;
        }

        void AttemptAutoCompletion(string value)
        {
            int characterPosition = Math.Max(0,input.caretPosition-2);

  
            // Don't show the suggestion box if the field is empty
            if (string.IsNullOrEmpty(value))
            {
                autoCompletionBox.gameObject.SetActive(false);
                return;
            }

            
            autoCompletionBox.gameObject.SetActive(true);

            // Clear the list
            foreach (Transform child in autoCompletionSuggestions.transform)
                Destroy(child.gameObject);

            // Re-populate it with updated suggestions
            // Disable the suggestion box if there are no suggestions
            Compendium compendium = Watchman.Get<Compendium>();



            List<AutoCompletionSuggestion> suggestions = new List<AutoCompletionSuggestion>();
            if (MakeElementSuggestions)
                suggestions.AddRange(GetElementAutoCompletionSuggestions(compendium, value));


            if (MakeRecipeSuggestions)
                suggestions.AddRange(GetRecipeAutoCompletionSuggestions(compendium, value));

            if (MakeEndingSuggestions)
                suggestions.AddRange(GetEndingAutoCompletionSuggestions(compendium, value));

            var orderedSuggestions = suggestions.OrderBy(acs => acs.GetText()).ToList();

            if (orderedSuggestions.Count == 0)
            {
                autoCompletionBox.gameObject.SetActive(false);
                return;
            }

            foreach (var suggestion in orderedSuggestions)
                suggestion.transform.SetParent(autoCompletionSuggestions.transform, false);

            var characterInfo = input.textComponent.textInfo.characterInfo;
            SetAutosuggestBoxPositionToMatchRecentlyTypedCharacter(characterInfo[characterPosition]);

        }

        public void SetAutosuggestBoxPositionToMatchRecentlyTypedCharacter(TMP_CharacterInfo character)
        {
            var characterBottomLeftWithOffset=new Vector3(character.bottomLeft.x,character.bottomLeft.y-20,character.bottomLeft.z);

            var positionConvertedFromPositionInInputFieldToWorld = input.transform.TransformPoint(characterBottomLeftWithOffset);

            autoCompletionBox.RectTransform.position = positionConvertedFromPositionInInputFieldToWorld;

        }


        public void SetInput(string text)
        {
            // Do nothing if it's not open
            if (!isActiveAndEnabled || text == null)
                return;

         
            input.text = text;
            lastAutocompleteCheckedTextValue = text; //so we don't trigger an autocomplete attempt
        }


        void ApplySuggestion(string suggestion)
        {
            SetInput(suggestion);
            autoCompletionBox.gameObject.SetActive(false);
        }

        List<AutoCompletionSuggestion> GetElementAutoCompletionSuggestions(Compendium compendium, string prompt)
        {
            return compendium.GetEntitiesAsList<Element>().
                Where(e => e.Id.StartsWith(prompt)).Select(e => MakeAutocompleteSuggestion(compendium, e.Id, typeof(Element))).ToList();
        }

        List<AutoCompletionSuggestion> GetRecipeAutoCompletionSuggestions(Compendium compendium, string prompt)
        {
            return compendium.GetEntitiesAsList<Recipe>().
                Where(r => r.Id.StartsWith(prompt)).Select(r => MakeAutocompleteSuggestion(compendium, r.Id, typeof(Recipe))).ToList();
        }

        List<AutoCompletionSuggestion> GetEndingAutoCompletionSuggestions(Compendium compendium, string prompt)
        {
            return compendium.GetEntitiesAsList<Ending>().
                Where(x => x.Id.StartsWith(prompt)).Select(x => MakeAutocompleteSuggestion(compendium, x.Id, typeof(Ending))).ToList();
        }

        AutoCompletionSuggestion MakeAutocompleteSuggestion(Compendium compendium, string suggestedId, Type entityType)
        {
            AutoCompletionSuggestion suggestion = Instantiate(AutoCompletionSuggestionPrefab).GetComponent<AutoCompletionSuggestion>();
            suggestion.SetText(suggestedId);
            suggestion.AddClickListener(() => ApplySuggestion(suggestedId));

            // Show the element image if applicable
            if (entityType==typeof(Element))
            {
                suggestion.SetIconForElement(compendium.GetEntityById<Element>(suggestedId));
                suggestion.SetCategoryText('E');
            }

            else if(entityType==typeof(Recipe))
                suggestion.SetCategoryText('R');

            else if(entityType==typeof(Ending))
                suggestion.SetCategoryText('X');

            return suggestion;
        }
    }
}
