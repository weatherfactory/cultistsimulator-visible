using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Commands
{
    public class SituationInterruptCommand
    {
        public bool Start { get; set; }
        public bool Halt { get; set; }

        public SituationInterruptCommand()
        {
            Start = false;
            Halt = false;

        }
    }
}
