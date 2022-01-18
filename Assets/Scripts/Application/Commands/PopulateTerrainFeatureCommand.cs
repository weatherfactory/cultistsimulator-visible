using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using Steamworks;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateTerrainFeatureCommand: ITokenPayloadCreationCommand, IEncaustment
    {
        public string Id { get; set; }
        public List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();
        public List<PopulateDominionCommand> Dominions { get; set; }
        public Dictionary<string, int> Mutations { get; set; }
        public bool IsOpen { get; set; }
        public int Quantity { get; set; }
        public PopulateTerrainFeatureCommand()
        {

        }
        public ITokenPayload Execute(Context context)
        {
          var existingTerrainFeatureToken=Watchman.Get<HornedAxe>().FindSingleOrDefaultTokenById("");
          //TODO: populate with the properties above!
          return existingTerrainFeatureToken.Payload;
        }

        
        
    }
}
