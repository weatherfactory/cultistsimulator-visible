using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace TabletopUi.Scripts.Elements
{
    public class AspectSetsDisplay : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private bool isWithinDetailsWindow;
        [SerializeField] private bool hideIfEmpty;
        [SerializeField] private bool hasBrightBackground;
        [SerializeField] private Transform[] setPrefixes;
        [SerializeField] private Transform aspectsDisplay;
#pragma warning restore 649
        private readonly List<IAspectsDictionary> _aspectSets = new List<IAspectsDictionary>();

        public void AddAspectSet(int setIndex, IAspectsDictionary aspects)
        {
            var requiredExpansion = setIndex + 1 - _aspectSets.Count;
            if (requiredExpansion > 0)
            {
                // Expand the number of sets as necessary
                _aspectSets.AddRange(new IAspectsDictionary[requiredExpansion]);
            }

            _aspectSets[setIndex] = aspects;
            RebuildAspectsDisplay();
        }

        public void Clear()
        {
            foreach (Transform child in aspectsDisplay.transform)
            {
                Destroy(child.gameObject);
            }
            gameObject.SetActive(!hideIfEmpty);
        }

        private void RebuildAspectsDisplay()
        {
            Clear();

            var numAspects = 0;
            for (var i = 0; i < _aspectSets.Count; i++)
            {
                var aspectSet = _aspectSets[i];
                if (aspectSet == null || aspectSet.Count == 0)
                {
                    continue;
                }

                if (setPrefixes[i] != null)
                {
                    Instantiate(setPrefixes[i], aspectsDisplay, false);
                }
                foreach (var k in aspectSet.Keys)
                {
                    AddAspectToDisplay(k, aspectSet[k]);
                    numAspects++;
                }
            }
            gameObject.SetActive(!hideIfEmpty || numAspects > 0);
        }

        private void AddAspectToDisplay(string aspectId, int quantity)
        {
            Element aspect = Registry.Retrieve<ICompendium>().GetElementById(aspectId);

            if (aspect == null)
                return; // We can't find the aspect? Well then don't add anything

            if (aspect.IsHidden)
                return; //...because it's hidden

            ElementFrame newElementFrame = PrefabFactory.CreateLocally<ElementFrame>(aspectsDisplay);
            newElementFrame.PopulateDisplay(aspect, quantity, null, hasBrightBackground);

            if (isWithinDetailsWindow)
                newElementFrame.SetAsDetailWindowChild();
        }
    }
}
