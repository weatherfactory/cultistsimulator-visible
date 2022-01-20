using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.Spheres.Dominions;
using SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class InitialiseAsTerrainFeature: MonoBehaviour
    {
        public string EditableIdentifier;


        public void Initialise(Sphere inSphere)
        {
         //   var token = gameObject.GetComponent<Token>();
         var token = gameObject.AddComponent<Token>();
         var existingManifestation = gameObject.AddComponent<MinimalManifestation>();
            var terrainFeaturePayload = new TerrainFeature();
            terrainFeaturePayload.SetId(EditableIdentifier);
            token.SetPayloadWithExistingManifestation(terrainFeaturePayload, existingManifestation);
            
            inSphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));

            var dominionComponentsInChildren = gameObject.GetComponentsInChildren<WorldDominion>();
            foreach(var d in dominionComponentsInChildren)
                d.RegisterFor(terrainFeaturePayload); //this will also attach the spheres.

        }
    }
}
