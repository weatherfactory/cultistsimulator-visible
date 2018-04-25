using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.Core.Services
{
    /// <summary>
    /// meta responses to significant in game events
    /// </summary>
    
    
    public class Chronicler
    {
        private IGameEntityStorage _storage;
        private const string BOOK_ASPECT = "text";
        private const string DESIRE_ASPECT = "desire";



        public Chronicler(IGameEntityStorage storage)
        {
            _storage = storage;
        }

        public void CharacterNameChanged(string newName)
        {
            _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastCharacterName,newName);
        }

        public void TokenPlacedOnTabletop(ElementStackToken token)
        {
            IAspectsDictionary tokenAspects = token.GetAspects();

            if(tokenAspects.Keys.Contains(BOOK_ASPECT))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastBook,token.Id);

            if (tokenAspects.Keys.Contains(DESIRE_ASPECT))
            { 
                if(token.Id.Contains("sensation"))
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionsensationa");
                else if (token.Id.Contains("power"))
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionpowera");
                else if(token.Id.Contains("enlightenment"))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionenlightenmenta");
            }

        }

    }
}
