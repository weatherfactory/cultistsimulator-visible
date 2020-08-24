using System;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;

namespace Assets.Core.Services
{
    /// <summary>
    /// tokens as in arbitrary
    /// </summary>
    public class TokenReplacer
    {
        private Character _character;
        private ICompendium _compendium;
        //'token' as in text-to-be-replaced, not as in DraggableToken
        public TokenReplacer(Character ch,ICompendium co)
        {
            _character = ch;
            _compendium = co;
        }

        public string ReplaceTextFor(string text)
        {
            
            string previousCharacterName = _character.GetPastLegacyEventRecord(LegacyEventRecordId.lastcharactername.ToString());
            string lastFollowerId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.lastfollower.ToString());
            string lastDesireId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString());
            string lastBookId = _character.GetPastLegacyEventRecord(LegacyEventRecordId.lastbook.ToString());
            string lastBookLabel = string.Empty;
            string lastDesireLabel=String.Empty;
            string lastFollowerLabel = String.Empty;

			if (previousCharacterName == HistoryBuilder.DEFAULT_CHARACTER_NAME)
			{
				previousCharacterName = Registry.Get<ILanguageManager>().Get("UI_DEFAULTNAME");	// Replace [unnamed] with a nicer default such as J.N.Sinombre - CP
			}

            var lastBook = _compendium.GetEntityById<Element>(lastBookId);
            if (lastBook == null)
                NoonUtility.Log("Duff elementId in PastLegacyEventRecord: " + lastBookId, 1);
            else lastBookLabel = lastBook.Label;

            var lastDesire = _compendium.GetEntityById<Element>(lastDesireId);
            if (lastDesire == null)
                NoonUtility.Log("Duff elementId in PastLegacyEventRecord: " + lastDesireId, 1);
            else lastDesireLabel = lastDesire.Label;

            var lastFollower = _compendium.GetEntityById<Element>(lastFollowerId);
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