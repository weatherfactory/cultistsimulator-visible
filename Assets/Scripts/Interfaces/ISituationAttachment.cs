using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;

namespace Assets.Scripts.Interfaces
{
    public interface ISituationAttachment
    {
        bool MatchesCommandCategory(CommandCategory category);
        void ClearThresholds();
        void CreateThreshold(SlotSpecification spec);
    }
}
