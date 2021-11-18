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
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class DropzoneCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public string Id { get; set; }

        public string EntityId { get; set; }
        public int Quantity { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }

        public DropzoneCreationCommand():this(typeof(ElementStack).ToString())
        {
        }

        public DropzoneCreationCommand(string entityId)
        {
            Dominions = new List<PopulateDominionCommand>();
            EntityId=entityId;
            Id = $"dropzone_{entityId}";
        }

        public ITokenPayload Execute(Context context)
        {
            var dz= new Dropzone(Id);
            foreach (var d in Dominions)
                    d.Execute(dz);

            return dz;
        }

        
    }
}

