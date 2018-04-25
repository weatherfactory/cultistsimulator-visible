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
        //'token' as in text-to-be-replaced, not as in DraggableToken
        public TokenReplacer(IGameEntityStorage c)
        {
            _character = c;
        }

        public string ReplaceTextFor(string text)
        {
            var previousCharacterName = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastCharacterName);
            var lastDesire = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastDesire);
            var lastBook = _character.GetPastLegacyEventRecord(LegacyEventRecordId.LastBook);

            if (text == null)
                return null; //huh. It really shouldn't be - I should be assigning empty string on load -  and yet sometimes it is. This is a guard clause to stop a basic nullreferenceexception
            string replaced = text;

            replaced= replaced.Replace(NoonConstants.TOKEN_PREVIOUS_CHARACTER_NAME, previousCharacterName);

            replaced = replaced.Replace(NoonConstants.TOKEN_LAST_DESIRE, lastDesire);
            replaced = replaced.Replace(NoonConstants.TOKEN_LAST_BOOK, lastBook);

            return replaced;
        }
    }
}