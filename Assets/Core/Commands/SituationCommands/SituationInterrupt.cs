using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Commands
{

    public enum SituationInterruptInput
    {
        Start,
        Halt
    }

    public class SituationInterrupt
    {
        public SituationInterruptInput SituationInterruptInput { get; private set; }

        public SituationInterrupt(SituationInterruptInput input)
        {
            SituationInterruptInput = input;
        }
    }

}
