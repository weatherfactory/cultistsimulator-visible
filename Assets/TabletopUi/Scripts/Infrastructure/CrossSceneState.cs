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
        private static MetaInfo _metaInfo;


        /// <summary>
        /// This is currently set, rather casually, in the MenuScreenController
        /// If at some point it contains more than the version number, we'll want to move it.
        /// </summary>
        /// <param name="metaInfo"></param>
        public static void SetMetaInfo(MetaInfo metaInfo)
        {
            _metaInfo = metaInfo;
        }

        public static MetaInfo GetMetaInfo()
        {
            return _metaInfo;
        }



        public static Hashtable GetSaveDataForCrossSceneState(Legacy activeLegacy)
        {

            var ht=new Hashtable();
            AddMetaInfoToHashtable(ht);
            AddCurrentEndingToHashtable(ht);
            AddAvailableLegaciesToHashtable(ht,activeLegacy);
            AddDefunctCharacterToHashtable(ht);

            return ht;
        }



        private static void AddMetaInfoToHashtable(Hashtable ht)
        {

            var htMetaInfo = new Hashtable {{SaveConstants.SAVE_VERSIONNUMBER, _metaInfo.VersionNumber}};
            ht.Add(SaveConstants.SAVE_METAINFO,htMetaInfo);

        }


        private static void AddDefunctCharacterToHashtable(Hashtable ht)
        {
            var c = GetDefunctCharacter();
            if (c != null)
            {
                var htDC = new Hashtable();
                htDC.Add(SaveConstants.SAVE_NAME,c.Name);
                
                var htFutureLevers=new Hashtable();
                foreach (var record in c.GetAllFutureLegacyEventRecords())
                    htFutureLevers.Add(record.Key.ToString(), record.Value);

                htDC.Add(SaveConstants.SAVE_FUTURE_LEVERS, htFutureLevers);

                ht.Add(SaveConstants.SAVE_DEFUNCT_CHARACTER_DETAILS, htDC);
            }            
        }



        private static void AddCurrentEndingToHashtable(Hashtable ht)
        {
            var currentEnding = GetCurrentEnding();
            if (currentEnding != null)
                ht.Add(SaveConstants.SAVE_CURRENTENDING, GetCurrentEnding().Id);
        }

        private static void AddAvailableLegaciesToHashtable(Hashtable ht,Legacy activeLegacy)
        {
            var htLegacies = new Hashtable();
            var availableLegacies = GetAvailableLegacies();

            if (activeLegacy != null)
            {
                //the player is in an active game, but we've just saved their game as inactive. This is most likely because 
                //we are saving an inactive game to restart it immediately. Make this the only available legacy, so the trap below
                //doesn't worry.
                htLegacies.Add(activeLegacy.Id,activeLegacy.Id);
            }

            else if (availableLegacies==null || !availableLegacies.Any())
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

        public static void ClearEnding()
        {
            _currentEnding = null;
        }

        public static void ClearLegacies()
        {
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
