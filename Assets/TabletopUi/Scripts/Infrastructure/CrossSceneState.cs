using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

   public static class CrossSceneState
    {
        private static Ending _currentEnding;
        private static List<Legacy> _availableLegacies;
        private static Legacy _chosenLegacy;
        //record the previous character when ending a game, so we can use their deets for setting the scene / recording in ongoing save
        private static Character _defunctCharacter;



        public static Hashtable GetHashTableForCrossSceneState()
        {
            var ht=new Hashtable();
            AddMetaInfoToHashtable(ht);
            AddCurrentEndingToHashtable(ht);
            AddAvailableLegaciesToHashtable(ht);
            AddDefunctCharacterToHashtable(ht);

            return ht;
        }

        private static void AddMetaInfoToHashtable(Hashtable ht)
        {
            
        }

        private static void AddDefunctCharacterToHashtable(Hashtable ht)
        {
            var c = GetDefunctCharacter();
            if (c != null)
            {
                var htDC = new Hashtable {{SaveConstants.SAVE_NAME, c.Name}};
                ht.Add(SaveConstants.SAVE_DEFUNCT_CHARACTER_DETAILS, htDC);
            }            
        }

        private static void AddCurrentEndingToHashtable(Hashtable ht)
        {
            var currentEnding = GetCurrentEnding();
            if (currentEnding != null)
                ht.Add(SaveConstants.SAVE_CURRENTENDING, GetCurrentEnding().Id);
        }

        private static void AddAvailableLegaciesToHashtable(Hashtable ht)
        {
            var htLegacies = new Hashtable();
            var availableLegacies = GetAvailableLegacies();

            if (availableLegacies==null && !availableLegacies.Any())
            {
                throw new ApplicationException("Tried to save game with no available legacies. This should never happen, and if it does, it'll corrupt your save. Please let us know what happened at " + NoonConstants.SupportEmail + " - thank you!");
            }
            else
            {
                foreach (var l in GetAvailableLegacies())
                {
                    htLegacies.Add(l.Id, l.Id);
                }
            }
            
            ht.Add(SaveConstants.SAVE_AVAILABLELEGACIES, htLegacies);
        }

        public static void SetCurrentEnding(Ending ending)
        {
            if(ending==null)
                throw new ApplicationException("Guard clause: use ClearEndingAndLegacies to set ending to null");
            _currentEnding = ending;
        }

        public static string GetDefunctCharacterName()
        {
            if (_defunctCharacter != null)
                return _defunctCharacter.Name;
            else
            return String.Empty;
        }


        public static Character GetDefunctCharacter()
        {
            return _defunctCharacter;
        }

        public static void SetDefunctCharacter(Character c)
        {
            _defunctCharacter = c;
        }

        public static Ending GetCurrentEnding()
        {
            return _currentEnding;
        }

        public static void ClearEndingAndLegacies()
        {
            _currentEnding = null;
            _availableLegacies = null;
        }

        public static void SetChosenLegacy(Legacy chosen)
        {
            _chosenLegacy = chosen;
        }


        public static Legacy GetChosenLegacy()
        {
            return _chosenLegacy;
        }


        public static List<Legacy> GetAvailableLegacies()
        {

            return _availableLegacies;
        }

        public static void SetAvailableLegacies(List<Legacy> legacies)
        {
            _availableLegacies = legacies;
        }


    }
}
