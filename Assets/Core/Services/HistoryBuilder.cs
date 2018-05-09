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
        public const string DEFAULT_CHARACTER_NAME = "J.N. Sinombre";
        public const string DEFAULT_LAST_BOOK = "textbooksanskrit";
        public const string DEFAULT_LAST_DESIRE = "ascensionsensationa";
        public const string DEFAULT_LAST_TOOL = "toolknockb";

        public const string DEFAULT_LAST_SIGNIFICANTPAINTING = "paintingmansus";
        public const string DEFAULT_LAST_CULT = "cultgrail_1";
        public const string DEFAULT_LAST_HEADQUARTERS = "generichq";
        public const string DEFAULT_LAST_PERSON_KILLED = "neville_a";
        public const string DEFAULT_FOLLOWER_AT_GAME_END = "rose_b";

        public Dictionary<LegacyEventRecordId, string> FillInDefaultPast(Dictionary<LegacyEventRecordId, string> currentPast)
        {
            Dictionary<LegacyEventRecordId, string> populatedPast;

            if (currentPast == null)
                populatedPast = new Dictionary<LegacyEventRecordId, string>();
            else
                populatedPast = currentPast;
            if(!populatedPast.ContainsKey(LegacyEventRecordId.LastCharacterName))
            populatedPast.Add(LegacyEventRecordId.LastCharacterName, DEFAULT_CHARACTER_NAME);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastBook))
                populatedPast.Add(LegacyEventRecordId.LastBook, DEFAULT_LAST_BOOK);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastDesire))
                populatedPast.Add(LegacyEventRecordId.LastDesire, DEFAULT_LAST_DESIRE);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastTool))
                populatedPast.Add(LegacyEventRecordId.LastTool, DEFAULT_LAST_TOOL);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastSignificantPainting))
                populatedPast.Add(LegacyEventRecordId.LastSignificantPainting, DEFAULT_LAST_SIGNIFICANTPAINTING);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastCult))
                populatedPast.Add(LegacyEventRecordId.LastCult, DEFAULT_LAST_CULT);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastHeadquarters))
                populatedPast.Add(LegacyEventRecordId.LastHeadquarters, DEFAULT_LAST_HEADQUARTERS);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastPersonKilled))
            populatedPast.Add(LegacyEventRecordId.LastPersonKilled, DEFAULT_LAST_PERSON_KILLED);
            if (!populatedPast.ContainsKey(LegacyEventRecordId.LastFollower))
                populatedPast.Add(LegacyEventRecordId.LastFollower, DEFAULT_FOLLOWER_AT_GAME_END);


            return populatedPast;
        }
    }
}
