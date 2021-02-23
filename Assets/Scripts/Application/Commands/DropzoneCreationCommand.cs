using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities.Verbs;

namespace SecretHistories.Commands
{
    public class DropzoneCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public int Quantity { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }

        public ITokenPayload Execute(Context context)
        {


            return new Dropzone();
        }

        
    }
}
