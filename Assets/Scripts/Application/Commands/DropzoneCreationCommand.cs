using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Enums;
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

        public DropzoneCreationCommand():this(nameof(ElementStack))
        {
        }

        public DropzoneCreationCommand(string entityId)
        {
            Dominions = new List<PopulateDominionCommand>();
            EntityId=entityId;
            Id = $"dropzone_{entityId}";

            var bubbleSphereSpec = new SphereSpec(typeof(BubbleSphere), $"{Id}bubble");
            Dominions.Add(new PopulateDominionCommand(SituationDominionEnum.Unknown.ToString(), bubbleSphereSpec));

        }

        public ITokenPayload Execute(Context context)
        {
            var dz= new Dropzone(Id,EntityId);
            foreach (var d in Dominions)
                    d.Execute(dz);

            return dz;
        }

        
    }
}

