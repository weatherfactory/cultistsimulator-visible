using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;

namespace Assets.Core.Entities
{
    public class ContainerMatchForStack
    {
        public IEnumerable<string> ProblemAspectIds = new List<string>();
        public SlotMatchForAspectsType MatchType { get; set; }

        public static ContainerMatchForStack MatchOK()
        {
            return new ContainerMatchForStack(new List<string>(), SlotMatchForAspectsType.Okay);
        }

        public ContainerMatchForStack(IEnumerable<string> problemAspectIds, SlotMatchForAspectsType esm)
        {
            ProblemAspectIds = problemAspectIds;
            MatchType = esm;
        }

        public string GetProblemDescription(Compendium compendium)
        {
            string description = "";
            if (MatchType == SlotMatchForAspectsType.ForbiddenAspectPresent)
            {
                string problemAspects = ProblemAspectsDescription(compendium);
                description += Registry.Get<ILocStringProvider>().Get("UI_ASPECTSFORBIDDEN") + problemAspects;
            }

            if (MatchType == SlotMatchForAspectsType.RequiredAspectMissing)
            {
                string problemAspects = ProblemAspectsDescription(compendium);
                description += Registry.Get<ILocStringProvider>().Get("UI_ASPECTSREQUIRED") + problemAspects;
            }

            return description;
        }

        public  string ProblemAspectsDescription(Compendium compendium)
        {
            string problemAspects = "";
            foreach (var problemAspectId in ProblemAspectIds)
            {
                if (problemAspects != "")
                    problemAspects += Registry.Get<ILocStringProvider>().Get("UI_OR");
                problemAspects += compendium.GetEntityById<Element>(problemAspectId).Label;
            }
			problemAspects += Registry.Get<ILocStringProvider>().Get("UI_FULLSTOP");
            return problemAspects;
        }
    }
}
