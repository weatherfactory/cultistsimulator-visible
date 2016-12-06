using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Mono.Cecil;
using Noon;
using OrbCreationExtensions;
using UnityEngine.Assertions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataHandler
    {
        Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations);
        void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave);
        Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks);
    }

    public class GameDataHandler : IGameDataHandler
    {
        public const string SAVE_ELEMENTID = "elementId";
        public const string SAVE_QUANTITY = "quantity";
        public const string SAVE_VERBID = "verbId";
        public const string SAVE_RECIPEID = "recipeId";
        public const string SAVE_SITUATIONSTATE = "state";
        public const string SAVE_TIMEREMAINING = "timeremaining";
        public const string SAVE_STARTINGSLOTELEMENTS = "startingslotelements";
        public const string SAVE_RECIPESKNOWN = "recipesKnown";
        public const string SAVE_ELEMENTSTACKS = "elementStacks";
        public const string SAVE_SITUATIONS = "situations";
        public const string SAVE_CHARACTER_DETAILS = "characterDetails";

        private ICompendium compendium;

        
        public GameDataHandler(ICompendium compendium)
        {
            this.compendium = compendium;
        }


        public Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
       {
           var htElementStacks=new Hashtable();
           foreach (var e in stacks)
           {
                var htStackProperties=new Hashtable();
               htStackProperties.Add(SAVE_ELEMENTID,e.Id);
                htStackProperties.Add(SAVE_QUANTITY, e.Quantity);
                htElementStacks.Add(e.SaveLocationInfo,htStackProperties);   
           }
           return htElementStacks;
       }

        private Hashtable ExportSituations(IEnumerable<ISituationAnchor> situations)
        {
            //states, slot contents, storage contents
            //window slot contents
            //notes and element contents

            var htSituations = new Hashtable();
            foreach (var s in situations)
            {
                var htSituationProperties = s.GetSaveDataForSituation();
                htSituations.Add(s.SaveLocationInfo, htSituationProperties);
            }
            return htSituations;
        }



       public Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
       {
           var htAll = new Hashtable
           {
               {SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
               {SAVE_SITUATIONS, ExportSituations(situations)}
           };
           return htAll;
       }

        public void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            var htElementStacks = htSave.GetHashtable(SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SAVE_SITUATIONS);

            ImportTabletopElementStacks(tabletopContainer, htElementStacks);

            ImportSituations(tabletopContainer, htSituations);

        }

        private static void ImportTabletopElementStacks(TabletopContainer tabletopContainer, Hashtable htElementStacks)
        {
            foreach (var locationInfo in htElementStacks.Keys)
            {
                var elementStacks =
                    NoonUtility.HashtableToStringStringDictionary(htElementStacks.GetHashtable(locationInfo));

                int quantity;
                var couldParse = Int32.TryParse(elementStacks[SAVE_QUANTITY], out quantity);
                if (!couldParse)
                    throw new ArgumentException("Couldn't parse " + elementStacks[SAVE_QUANTITY] + " for " +
                                                elementStacks[SAVE_ELEMENTID] + " as a valid quantity.");

                tabletopContainer.GetElementStacksManager()
                    .IncreaseElement(elementStacks[SAVE_ELEMENTID], quantity, locationInfo.ToString());
            }
        }

        private void ImportSituations(TabletopContainer tabletopContainer, Hashtable htSituations)
        {
            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);

                IVerb situationVerb = compendium.GetVerbById(htSituationValues[SAVE_VERBID].ToString());

                string recipeId = TryGetStringFromHashtable(htSituationValues, SAVE_RECIPEID);
                var recipe = compendium.GetRecipeById(recipeId);

                var command = new SituationCreationCommand(situationVerb, recipe);
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SAVE_TIMEREMAINING);
                command.State = TryGetNullableSituationState(htSituationValues, SAVE_SITUATIONSTATE);

               var situationAnchor= tabletopContainer.CreateSituation(command, locationInfo.ToString());

                AddElementStacksToStartingSlots_ForSituation(htSituationValues, situationAnchor,tabletopContainer);
            }
        }

        private void AddElementStacksToStartingSlots_ForSituation(Hashtable htSituationValues,
            ISituationAnchor situationAnchor, TabletopContainer tabletopContainer)
        {
            if (htSituationValues.ContainsKey(SAVE_STARTINGSLOTELEMENTS))
            {
                var htElements = htSituationValues.GetHashtable(SAVE_STARTINGSLOTELEMENTS);
                var elementQuantitySpecifications = new List<ElementQuantitySpecification>();
                foreach (var locationInfo in htElements.Keys)
                {
                    var elementValues =
                        NoonUtility.HashtableToStringStringDictionary(htElements.GetHashtable(locationInfo));
                    elementQuantitySpecifications.Add(new ElementQuantitySpecification(elementValues[SAVE_ELEMENTID],
                        GetQuantityFromElementHashtable(elementValues), locationInfo.ToString()));
                }
                
                foreach (var eqs in elementQuantitySpecifications.OrderBy(spec=>spec.Depth))
                {
                    var stackToPutInSlot =
                        tabletopContainer.GetTokenTransformWrapper()
                            .ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity);
                    var slotToFill = situationAnchor.GetSlotBySaveLocationInfoPath(eqs.LocationInfo);
                    if (slotToFill == null)
                        throw new ApplicationException("Can't find slot with locationinfo " + eqs.LocationInfo);

                    slotToFill.AcceptStack(stackToPutInSlot);
                }
            }
        }

        private int GetQuantityFromElementHashtable(Dictionary<string, string> elementValues)
        {
            int quantity;
            var couldParse = Int32.TryParse(elementValues[SAVE_QUANTITY], out quantity);
            if (!couldParse)
                throw new ArgumentException("Couldn't parse " + elementValues[SAVE_QUANTITY] + " for " +
                                            elementValues[SAVE_ELEMENTID] + " as a valid quantity.");
            return quantity;
        }

        private string TryGetStringFromHashtable(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            return ht[key].ToString();
        }


        private float? TryGetNullableFloatFromHashtable(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            string jsonString = ht[key].ToString();
            float returnValue;
            if(Single.TryParse(jsonString, out returnValue))
                return returnValue;
            else
                return null;
        }

        private SituationState? TryGetNullableSituationState(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            return (SituationState)Enum.Parse(typeof(SituationState), ht[key].ToString());
        }

    }
    
}
