using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string GetProblemDescription()
        {
            string description = "";
            if (MatchType == SlotMatchForAspectsType.ForbiddenAspectPresent)
            {
                string problemAspects = ProblemAspectsDescription();
                description += "Elements with the " + problemAspects + " aspects are unacceptable here. *Unacceptable*.";
            }

            if (MatchType == SlotMatchForAspectsType.RequiredAspectMissing)
            {
                string problemAspects = ProblemAspectsDescription();
                description += "Only elements with the " + problemAspects + " aspects can go here.";
            }

            return description;
        }

        public  string ProblemAspectsDescription()
        {
            string problemAspects = "";
            foreach (var problemAspectId in ProblemAspectIds)
            {
                if (problemAspects != "")
                    problemAspects += " or ";
                problemAspects += problemAspectId;
            }
            return problemAspects;
        }
    }
}
