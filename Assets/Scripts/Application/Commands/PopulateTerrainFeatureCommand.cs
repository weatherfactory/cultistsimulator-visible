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

            //In most cases, we'd now recreate the payload. In this case, the initialiser has already done this at scene setup. We
            //just need to retrieve it and update its properties.
            var terrainFeaturePayload = existingTerrainFeatureToken.Payload as TerrainFeature;
       
          terrainFeaturePayload.Quantity = Quantity; //may always be meaningless, but let's assume there's some useful difference of degree.
          terrainFeaturePayload.IsOpen = IsOpen; //likely useful for info windows; we may need to call something to recognise its openness
          foreach(var m in Mutations)
              terrainFeaturePayload.SetMutation(m.Key,m.Value,false);


          foreach (var d in Dominions)
              d.Execute(terrainFeaturePayload);
          
          return existingTerrainFeatureToken.Payload;
        }

        
        
    }
}
