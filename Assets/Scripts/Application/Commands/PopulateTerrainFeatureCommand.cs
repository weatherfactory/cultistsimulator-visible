using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres.Dominions;
using SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads;
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
        public List<PopulateDominionCommand> Dominions { get; set; } //currently we run sphere commands directly without dominion intervention. That will likely change. Swap out below if so.
        public Dictionary<string, int> Mutations { get; set; }
        public bool IsOpen { get; set; }
        public int Quantity { get; set; }
        public PopulateTerrainFeatureCommand()
        {

        }
        public ITokenPayload Execute(Context context)
        {
            
          var existingTerrainFeatureToken=Watchman.Get<HornedAxe>().FindSingleOrDefaultTokenById(Id);
          var terrainFeaturePayload = new TerrainFeature();
          terrainFeaturePayload.SetId(Id); //unusually, because terrain features are 100% unique, the payload and the token have the same Id
          terrainFeaturePayload.Quantity = Quantity;
          terrainFeaturePayload.IsOpen = IsOpen;
          foreach(var m in Mutations)
              terrainFeaturePayload.SetMutation(m.Key,m.Value,false);

            //illuminations?
            var dominionComponentsInChildren = existingTerrainFeatureToken.gameObject.GetComponentsInChildren<WorldDominion>();

            foreach (var d in dominionComponentsInChildren)
                d.RegisterFor(terrainFeaturePayload); //this should also activate the spheres

            foreach (var d in Dominions)
              d.Execute(terrainFeaturePayload);
          
          return existingTerrainFeatureToken.Payload;
        }

        
        
    }
}
