using System;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Services
{
    /// <summary>
    /// tokens as in arbitrary
    /// </summary>
    public class TokenReplacer
    {
        private IGameEntityStorage _character;
        private ICompendium _compendium;
        //'token' as in text-to-be-replaced, not as in DraggableToken
        public TokenReplacer(IGameEntityStorage ch,ICompendium co)
        {
            _character = ch;
            _compendium = co;
        }

        public string ReplaceTextFor(string text)
        {
            
            string previousCharacterName = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastCharacterName);
            string lastFollowerId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastFollower);
            string lastDesireId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastDesire);
            string lastBookId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastBook);
            string lastBookLabel = string.Empty;
            string lastDesireLabel=String.Empty;
            string lastFollowerLabel = String.Empty;

			if (previousCharacterName == HistoryBuilder.DEFAULT_CHARACTER_NAME)
			{
				previousCharacterName = LanguageTable.Get("UI_DEFAULTNAME");	// Replace [unnamed] with a nicer default such as J.N.Sinombre - CP
			}

            var lastBook = _compendium.GetElementById(lastBookId);
            if (lastBook == null)
                NoonUtility.Log("Duff elementId in PastLegacyEventRecord: " + lastBookId, 1);
            else lastBookLabel = lastBook.Label;

            var lastDesire = _compendium.GetElementById(lastDesireId);
            if (lastDesire == null)
                NoonUtility.Log("Duff elementId in PastLegacyEventRecord: " + lastDesireId, 1);
            else lastDesireLabel = lastDesire.Label;

            var lastFollower = _compendium.GetElementById(lastFollowerId);
            if (lastFollower == null)
                NoonUtility.Log("Duff elementId in PastLegacyEventRecord: " + lastFollowerId, 1);
            else lastFollowerLabel = lastFollower.Label;


            if (text == null)
                return null; //huh. It really shouldn't be - I should be assigning empty string on load -  and yet sometimes it is. This is a guard clause to stop a basic nullreferenceexception
            string replaced = text;

            replaced= replaced.Replace(NoonConstants.TOKEN_PREVIOUS_CHARACTER_NAME, previousCharacterName);
            replaced = replaced.Replace(NoonConstants.TOKEN_LAST_FOLLOWER, lastFollowerLabel);

            replaced = replaced.Replace(NoonConstants.TOKEN_LAST_DESIRE, lastDesireLabel);
            replaced = replaced.Replace(NoonConstants.TOKEN_LAST_BOOK, lastBookLabel);

            return replaced;
        }
    }
}