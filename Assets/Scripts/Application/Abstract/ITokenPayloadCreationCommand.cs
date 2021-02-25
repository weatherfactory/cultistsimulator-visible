using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Fucine;
using SecretHistories.Spheres;

namespace SecretHistories.Abstract
{
    public interface ITokenPayloadCreationCommand
    {
        public ITokenPayload Execute(Context context);
        public int Quantity { get; }
        public List<PopulateDominionCommand> Dominions { get; set; }
    }
}
