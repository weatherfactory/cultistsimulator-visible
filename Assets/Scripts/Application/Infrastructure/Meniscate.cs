#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using Assets.Logic;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;

using SecretHistories.Enums.Elements;
using SecretHistories.Infrastructure;
using SecretHistories.NullObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace SecretHistories.UI {
    public class Meniscate : MonoBehaviour
    {

        [SerializeField] private CardHoverDetail cardHoverDetail;


        private List<Token> _multiSelectedTokens = new List<Token>();
        private Token _currentlyDraggedToken;

        public void Awake()
        {
            var registry = new Watchman();
            registry.Register(this);
            _currentlyDraggedToken = NullToken.Create();
        }

        public void Update()
        {
            Watchman.Get<Concursum>().DoUpdate();
        }



        public void CloseAllSituationWindowsExcept(string exceptVerbId) {
            var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.Verb.Id != exceptVerbId)
                    s.Close();
            }
        }

        public bool IsSituationWindowOpen() {
	        var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
	        return situations.Any(c => c.IsOpen);
        }

        public Situation GetCurrentlyOpenSituation()
        {
            var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            var openSituations = situations.Where(s => s.IsOpen);

            
            var enumerable = openSituations.ToList();
            if (enumerable.Count > 1)
            {
               string warning=$"Found {situations.Count} open situations, which shouldn't happen: ";
               foreach (var o in new List<Situation>(enumerable))
                   warning += $" {o.Id};";
               NoonUtility.LogWarning(warning);

               return enumerable.First();
            }

            if (enumerable.Count == 0)
                return NullSituation.Create();

            return enumerable.Single();

        }

        public void SetHighlightedElement(string elementId, int quantity = 1)
        {
            var enableAccessibleCards =
                Watchman.Get<Config>().GetConfigValueAsInt(NoonConstants.ACCESSIBLECARDS);

            if (enableAccessibleCards==null || enableAccessibleCards==0)
		        return;

            if (cardHoverDetail == null) //in BH, there's no CardHoverDetail reference set yet.
                return;
            
            if (elementId == null)
	        {
		        cardHoverDetail.Hide();
		        return;
	        }
	        cardHoverDetail.Populate(elementId, quantity);
	        cardHoverDetail.Show();
        }

        public void ToggleMultiSelectedToken(Token token)
        {
            if (token.IsValid() && _multiSelectedTokens.Contains(token))
            {
                _multiSelectedTokens.Remove(token);
                token.Deselect();
            }
            else if (token.IsValid())
            {
                _multiSelectedTokens.Add(token);
                token.Select();
            }
        }
        public void AddMultiSelectedToken(Token token)
        {
            if(token.IsValid() && !_multiSelectedTokens.Contains(token))
            {
                _multiSelectedTokens.Add(token);
                token.Select();
            }
        }
        public void ClearMultiSelectedToken(Token token)
        {
            if (token.IsValid() && _multiSelectedTokens.Contains(token))
            {
                _multiSelectedTokens.Remove(token);
                token.Select();
            }
                
        }

        public void ClearMultiSelectedTokens()
        {
            var tokensToClear = new List<Token>(_multiSelectedTokens);

            _multiSelectedTokens.Clear();

            foreach(var t in tokensToClear)
                t.Deselect();

        }

        public bool IsMultiSelected(Token token)
        {
            if (_multiSelectedTokens.Contains(token))
                return true;
            return false;

        }

        public void StartMultiDragAlong(Token draggedToken, PointerEventData eventData)
        {
            foreach (Token multiSelectedToken in _multiSelectedTokens)
            {
                if(multiSelectedToken.IsValid() && multiSelectedToken!=draggedToken)
                    multiSelectedToken.StartDragAlong(eventData,draggedToken);
            }
        }

        public void OnMultiDragAlong(Vector3 originalPosition, Token draggedToken)
        {
            foreach (Token multiSelectedToken in _multiSelectedTokens)
            {
                if (multiSelectedToken.IsValid() && multiSelectedToken != draggedToken)
                {
                    multiSelectedToken.ContinueDragAlong(originalPosition, draggedToken);

                    //display ghosts for dragged tokens
                    //This only makes sense in Cultist classic, where we want to put multiselected tokens exclusively in the
                    //tabletop sphere. If we want to put them in bookcases or containers, it'll need more flexibility
                    var tabletopSphere = Watchman.Get<HornedAxe>().GetDefaultSphere(); 
                    tabletopSphere.TryDisplayGhost(multiSelectedToken);
                }
            }

            
        }
 
        public void OnMultiEndDrag(PointerEventData eventData, Token draggedToken)
        {
            foreach (Token multiSelectedToken in _multiSelectedTokens)
            {
                if (multiSelectedToken.IsValid() && multiSelectedToken != draggedToken)
                    multiSelectedToken.EndDragAlong(eventData, draggedToken);
            }
            ClearMultiSelectedTokens();
        }

        public void SetCurrentlyDraggedToken(Token token)
        {
            _currentlyDraggedToken = token;
        }
        public void ClearCurrentlyDraggedToken()
        {
            _currentlyDraggedToken = NullToken.Create();
        }

        public Token GetCurrentlyDraggedToken()
        {
            return _currentlyDraggedToken;

        }

    }


}
