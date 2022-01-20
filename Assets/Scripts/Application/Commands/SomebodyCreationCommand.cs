using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class SomebodyCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public string Id { get; set; }
        public string EntityId { get; set; }

        public int Quantity { get; }
        public List<PopulateDominionCommand> Dominions { get; set; }
        public SomebodyCreationCommand()
        {
        }

        public SomebodyCreationCommand(string entityId)
        {
            EntityId=entityId;


        }

            public ITokenPayload Execute(Context context)
        {
            var compendium = Watchman.Get<Compendium>();
            var element = compendium.GetEntityById<Element>(EntityId);
            Id = element.DefaultUniqueTokenId();

            if (String.IsNullOrEmpty(Id))
                Id = element.DefaultUniqueTokenId();

            var newSomebody = new Somebody(Id, element);
            foreach (var d in Dominions)
                d.Execute(newSomebody);

            return newSomebody;
        }


    }
}
