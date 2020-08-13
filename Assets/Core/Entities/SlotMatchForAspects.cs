using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;

namespace Assets.Core.Entities
{
    public class SlotMatchForAspects
    {
        public IEnumerable<string> ProblemAspectIds = new List<string>();
        public SlotMatchForAspectsType MatchType { get; set; }

        public static SlotMatchForAspects MatchOK()
        {
            return new SlotMatchForAspects(new List<string>(), SlotMatchForAspectsType.Okay);
        }

        public SlotMatchForAspects(IEnumerable<string> problemAspectIds, SlotMatchForAspectsType esm)
        {
            ProblemAspectIds = problemAspectIds;
            MatchType = esm;
        }

        public string GetProblemDescription(ICompendium compendium)
        {
            string description = "";
            if (MatchType == SlotMatchForAspectsType.ForbiddenAspectPresent)
            {
                string problemAspects = ProblemAspectsDescription(compendium);
                description += Registry.Retrieve<LanguageManager>().Get("UI_ASPECTSFORBIDDEN") + problemAspects;
            }

            if (MatchType == SlotMatchForAspectsType.RequiredAspectMissing)
            {
                string problemAspects = ProblemAspectsDescription(compendium);
                description += Registry.Retrieve<LanguageManager>().Get("UI_ASPECTSREQUIRED") + problemAspects;
            }

            return description;
        }

        public  string ProblemAspectsDescription(ICompendium compendium)
        {
            string problemAspects = "";
            foreach (var problemAspectId in ProblemAspectIds)
            {
                if (problemAspects != "")
                    problemAspects += Registry.Retrieve<LanguageManager>().Get("UI_OR");
                problemAspects += compendium.GetEntityById<Element>(problemAspectId).Label;
            }
			problemAspects += Registry.Retrieve<LanguageManager>().Get("UI_FULLSTOP");
            return problemAspects;
        }
    }
}
