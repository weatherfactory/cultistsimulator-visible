using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataExporter
    {
        Hashtable GetSaveHashTable(Character character, IEnumerable<IElementStack> stacks, IEnumerable<SituationController> situationControllers,IEnumerable<IDeckInstance> decks);

        Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks);

        Hashtable GetHashTableForSituationNotes(IEnumerable<ISituationNote> notes);
        Hashtable GetHashtableForExtragameState();
}

    public class GameDataExporter: IGameDataExporter
    {

        /// <summary>
        /// top-level function, subfunctions wrap everything else
        /// Currently we only have stacks and situations passes in (each of those is then investigated); more entities would need more params
        /// </summary>
        /// <returns>a hashtable ready to be jsonised (or otherwise stored)</returns>
        public Hashtable GetSaveHashTable(Character character,IEnumerable<IElementStack> stacks, IEnumerable<SituationController> situationControllers,IEnumerable<IDeckInstance> deckInstances)
        {
            var htAll = new Hashtable()
            {
                {SaveConstants.SAVE_CHARACTER_DETAILS,GetHashTableForCharacter(character) },
                {SaveConstants.SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
                {SaveConstants.SAVE_SITUATIONS, GetHashTableForSituations(situationControllers)},
                {SaveConstants.SAVE_DECKS,GetHashTableForDecks(deckInstances) }
            };
            return htAll;
        }

        private Hashtable GetHashTableForCharacter(Character character)
        {
            var htCharacter=new Hashtable();
            htCharacter.Add(SaveConstants.SAVE_NAME,character.Name);
            htCharacter.Add(SaveConstants.SAVE_PROFESSION, character.Profession);
            htCharacter.Add(SaveConstants.SAVE_PREVIOUS_CHARACTER_NAME,character.PreviousCharacterName);

            var htExecutions=new Hashtable();
            foreach (var e in character.GetAllExecutions())
            {
                htExecutions.Add(e.Key,e.Value);
            }

            htCharacter.Add(SaveConstants.SAVE_EXECUTIONS,htExecutions);

            return htCharacter;
        }


        /// <summary>
        /// return save data for all ongoing situations. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are handled by this function
        /// At the moment, the functionality for this is in the tokens and controllers
        /// I think it probably shouldn't be
        /// </summary>
        /// <returns></returns>
        private Hashtable GetHashTableForSituations(IEnumerable<SituationController> situationControllers)
        {
   
            var htSituations = new Hashtable();
            foreach (var s in situationControllers)
            {
                var htSituationProperties = s.GetSaveData(); 
                htSituations.Add(s.situationToken.SaveLocationInfo, htSituationProperties);
            }
            return htSituations;
        }
        /// <summary>
        /// return save data for all stacks. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are *not* handled by this function
        /// </summary>
        /// <returns></returns>
        public Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
               var stackHashtable=GetHashtableForThisStack(e);

                    htElementStacks.Add(e.SaveLocationInfo, stackHashtable);

            }
            return htElementStacks;
        }

        public Hashtable GetHashTableForDecks(IEnumerable<IDeckInstance> deckInstances)
        {
            var htDecks=new Hashtable();
            foreach (var d in deckInstances)
            {
                var deckHashTable = d.GetSaveData();
                htDecks.Add(d.Id,deckHashTable);
            }
            return htDecks;
        }

        public Hashtable GetHashtableForExtragameState()
        {
            return CrossSceneState.GetHashTableForCrossSceneState();
        }

private Hashtable GetHashtableForThisStack(IElementStack e)
        {
            var htStackProperties = new Hashtable();
            htStackProperties.Add(SaveConstants.SAVE_ELEMENTID, e.Id);
            htStackProperties.Add(SaveConstants.SAVE_QUANTITY, e.Quantity);
            return htStackProperties;
        }

        public Hashtable GetHashTableForSituationNotes(IEnumerable<ISituationNote> notes)
        {
            var htNotes=new Hashtable();
            foreach (var on in notes)
            {
                var htEachNote = new Hashtable();
                htEachNote.Add(SaveConstants.SAVE_TITLE, on.Description);

                htNotes.Add((htNotes.Keys.Count + 1).ToString(), htEachNote); //need that tostring! exporter doesn't cope well with int keys
            }

            return htNotes;
        }
        
    }
}
