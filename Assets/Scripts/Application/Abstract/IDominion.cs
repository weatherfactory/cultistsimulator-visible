using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;

namespace SecretHistories.Interfaces
{
    public interface IDominion
    {
        bool MatchesCommandCategory(CommandCategory category);
        void ClearThresholds();
        void CreateThreshold(SlotSpecification spec);
    }
}
