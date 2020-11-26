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

        public Dictionary<string, string> FillInDefaultPast(Dictionary<string, string> currentPast)
        {
            Dictionary<string, string> populatedPast;

            if (currentPast == null)
                populatedPast = new Dictionary<string, string>();
            else
                populatedPast = currentPast;
            if(!populatedPast.ContainsKey(LegacyEventRecordId.lastcharactername.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastcharactername.ToString(), DEFAULT_CHARACTER_NAME);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastbook.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastbook.ToString(), DEFAULT_LAST_BOOK);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastdesire.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastdesire.ToString(), DEFAULT_LAST_DESIRE);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lasttool.ToString()))
                populatedPast.Add(LegacyEventRecordId.lasttool.ToString(), DEFAULT_LAST_TOOL);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastsignificantpainting.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastsignificantpainting.ToString(), DEFAULT_LAST_SIGNIFICANTPAINTING);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastcult.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastcult.ToString(), DEFAULT_LAST_CULT);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastheadquarters.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastheadquarters.ToString(), DEFAULT_LAST_HEADQUARTERS);

            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastpersonkilled.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastpersonkilled.ToString(), DEFAULT_LAST_PERSON_KILLED);
            if (!populatedPast.ContainsKey(LegacyEventRecordId.lastfollower.ToString()))
                populatedPast.Add(LegacyEventRecordId.lastfollower.ToString(), DEFAULT_FOLLOWER_AT_GAME_END);


            return populatedPast;
        }
    }
}
