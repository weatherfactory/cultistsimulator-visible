using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;

namespace Assets.Core.Entities
{
   public class MansusSituation: Situation
    {
        public MansusSituation(SituationCreationCommand command) : base(command)
        {
        }
    }
}
