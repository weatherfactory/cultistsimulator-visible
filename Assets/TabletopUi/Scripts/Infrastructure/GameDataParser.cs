using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataParser
    {
        Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations);
        void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave);
    }

    public class GameDataParser : IGameDataParser
    {
        private ICompendium compendium;

        
        public GameDataParser(ICompendium compendium)
        {
            this.compendium = compendium;
        }


        private Hashtable ExportElementStacks(IEnumerable<IElementStack> stacks)
       {
           var htElementStacks=new Hashtable();
           foreach (var e in stacks)
           {
                var htStackProperties=new Hashtable();
               htStackProperties.Add(NoonConstants.SAVE_ELEMENTID,e.Id);
                htStackProperties.Add(NoonConstants.SAVE_QUANTITY, e.Quantity);
                htElementStacks.Add(e.LocationInfo,htStackProperties);   
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
                var htSituationProperties = new Hashtable();

                htSituations.Add(s.LocationInfo, htSituationProperties);
                s.PopulateSaveInfo(htSituationProperties);
            }
            return htSituations;
        }



       public Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
       {
           var htAll = new Hashtable
           {
               {Noon.NoonConstants.SAVE_ELEMENTSTACKS, ExportElementStacks(stacks)},
               {Noon.NoonConstants.SAVE_SITUATIONS, ExportSituations(situations)}
           };
           return htAll;
       }

        public void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            var htElementStacks = htSave.GetHashtable(Noon.NoonConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(Noon.NoonConstants.SAVE_SITUATIONS);

            foreach (var locatorId in htElementStacks.Keys)
            {
                var elementStacks =
                    Noon.NoonUtility.HashtableToStringStringDictionary(htElementStacks.GetHashtable(locatorId));

                int quantity;
               var couldParse=int.TryParse(elementStacks[NoonConstants.SAVE_QUANTITY], out quantity);
                if(!couldParse)
                    throw new ArgumentException("Couldn't parse " + elementStacks[NoonConstants.SAVE_QUANTITY] + " for " + elementStacks[NoonConstants.SAVE_ELEMENTID] + " as a valid quantity.");

                tabletopContainer.GetElementStacksManager()
                    .IncreaseElement(elementStacks[NoonConstants.SAVE_ELEMENTID],quantity,locatorId.ToString());
            }


            foreach (var locatorId in htSituations.Keys)
            {
                var situationValues =
                    Noon.NoonUtility.HashtableToStringStringDictionary(htSituations.GetHashtable(locatorId));

                IVerb situationVerb = compendium.GetVerbById(situationValues[NoonConstants.SAVE_VERBID]);

                string recipeId;
                situationValues.TryGetValue(NoonConstants.SAVE_RECIPEID,out recipeId);
                var recipe = compendium.GetRecipeById(recipeId);

                var command = new SituationCreationCommand(situationVerb, recipe);
                command.TimeRemaining = TryGetNullableFloat(situationValues, NoonConstants.SAVE_TIMEREMAINING);
                command.State = TryGetNullableSituationState(situationValues, NoonConstants.SAVE_SITUATIONSTATE);

                tabletopContainer.CreateSituation(command, locatorId.ToString());

            }

        }

        private float? TryGetNullableFloat(Dictionary<string,string> d, string key)
        {
            string returnValue;
            d.TryGetValue(key,out returnValue);
            if (returnValue == null)
                return null;

            return float.Parse(returnValue);

        }

        private SituationState? TryGetNullableSituationState(Dictionary<string, string> d, string key)
        {
            string returnValue;
            d.TryGetValue(key, out returnValue);
            if (returnValue == null)
                return null;

            return (SituationState)Enum.Parse(typeof(SituationState), returnValue);

        }
    }
    
}
