using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Services
{
    /// <summary>
    /// supplies initial PastLegacyEventRecords
    /// </summary>
 public class HistoryBuilder
    {
        public const string DEFAULT_CHARACTER_NAME = "Sinombre";
        public const string DEFAULT_LAST_BOOK = "Sinombre";
        public const string DEFAULT_LAST_DESIRE = "ascensionsensationa";
        public const string DEFAULT_LAST_TOOL = "toolknockb";

        public const string DEFAULT_LAST_SIGNIFICANTPAINTING = "paintingmansus";
        public const string DEFAULT_LAST_CULT = "cultgrail_1";
        public const string DEFAULT_LAST_HEADQUARTERS = "generichq";
        public const string DEFAULT_LAST_PERSON_KILLED = "neville_a";
        
        public Dictionary<LegacyEventRecordId, string> SpecifyDefaultPast(Dictionary<LegacyEventRecordId, string> currentPast)
        {
            Dictionary<LegacyEventRecordId, string > populatedPast;

            if (currentPast == null)
                populatedPast = new Dictionary<LegacyEventRecordId, string>();
            else
                populatedPast = currentPast;
            populatedPast.Add(LegacyEventRecordId.LastCharacterName, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastBook, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastDesire, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastTool, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastSignificantPainting, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastCult, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastHeadquarters, DEFAULT_CHARACTER_NAME);
            populatedPast.Add(LegacyEventRecordId.LastPersonKilled, DEFAULT_CHARACTER_NAME);

            return populatedPast;
        }
    }
}
