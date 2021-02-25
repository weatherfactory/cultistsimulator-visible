using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities.Verbs;
using SecretHistories.Fucine;

namespace SecretHistories.Commands
{
    public class DropzoneCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public int Quantity { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }
        public FucinePath CachedParentPath { get; set; }

        public ITokenPayload Execute(Context context, FucinePath atSpherePath)
        {
            var dz= new Dropzone();
            return dz;
        }

        
    }
}
