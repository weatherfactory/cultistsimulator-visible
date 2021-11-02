using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Constants;

namespace SecretHistories.Services
{
    /// <summary>
    /// supplies initial PastLegacyEventRecords
    /// </summary>
 public class HistoryBuilder
    {
        
        public const string DEFAULT_LAST_BOOK = "textbooksanskrit";
        public const string DEFAULT_LAST_DESIRE = "ascensionsensationa";
        public const string DEFAULT_LAST_TOOL = "toolknockb";

        public const string DEFAULT_LAST_SIGNIFICANTPAINTING = "paintingmansus";
        public const string DEFAULT_LAST_CULT = "cultgrail_1";
        public const string DEFAULT_LAST_HEADQUARTERS = "generichq";
        public const string DEFAULT_LAST_PERSON_KILLED = "neville_a";
        public const string DEFAULT_FOLLOWER_AT_GAME_END = "rose_b";

        public Dictionary<string, string> FillInDefaultPast()
        {
            return FillInDefaultPast(new Dictionary<string, string>());
        }

        public Dictionary<string, string> FillInDefaultPast(Dictionary<string, string> currentPast)
        {
            Dictionary<string, string> populatedPast;

            if (currentPast == null)
                populatedPast = new Dictionary<string, string>();
            else
                populatedPast = currentPast;

            tryAddRecord(populatedPast,LegacyEventRecordId.lastcharactername.ToString(),NoonConstants.ARCHETYPICAL_CHARACTER_NAME);

            tryAddRecord(populatedPast, LegacyEventRecordId.lastbook.ToString(), DEFAULT_LAST_BOOK);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastdesire.ToString(), DEFAULT_LAST_DESIRE);
            tryAddRecord(populatedPast, LegacyEventRecordId.lasttool.ToString(), DEFAULT_LAST_TOOL);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastsignificantpainting.ToString(), DEFAULT_LAST_SIGNIFICANTPAINTING);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastcult.ToString(), DEFAULT_LAST_CULT);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastheadquarters.ToString(), DEFAULT_LAST_HEADQUARTERS);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastpersonkilled.ToString(), DEFAULT_LAST_PERSON_KILLED);
            tryAddRecord(populatedPast, LegacyEventRecordId.lastfollower.ToString(), DEFAULT_FOLLOWER_AT_GAME_END);

            return populatedPast;
        }

        public void tryAddRecord(Dictionary<string, string> records, string legacyEventRecordId, string value)
        {
            if (!records.ContainsKey(legacyEventRecordId))
            {
                records.Add(legacyEventRecordId, value);
                NoonUtility.Log($"HistoryBuilder Can't find a value for {legacyEventRecordId} - supplying '{value}'");
            }
        }
    }
}
