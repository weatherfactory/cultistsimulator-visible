using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataExporter
    {
        Hashtable ExportStacksAndSituations(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations);
        Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks);
    }

    public class GameDataExporter: IGameDataExporter
    {


        public Hashtable ExportStacksAndSituations(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
        {
            var htAll = new Hashtable
           {
               {GameSaveManager.SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
               {GameSaveManager.SAVE_SITUATIONS, GetHashTableForSituations(situations)}
           };
            return htAll;
        }

        private Hashtable GetHashTableForSituations(IEnumerable<ISituationAnchor> situations)
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

        public Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
                var htStackProperties = new Hashtable();
                htStackProperties.Add(GameSaveManager.SAVE_ELEMENTID, e.Id);
                htStackProperties.Add(GameSaveManager.SAVE_QUANTITY, e.Quantity);
                htElementStacks.Add(e.SaveLocationInfo, htStackProperties);
            }
            return htElementStacks;
        }
    }
}
