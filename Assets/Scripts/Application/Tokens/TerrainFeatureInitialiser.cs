using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.Tokens.TokenPayloads;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TerrainFeatureInitialiser: MonoBehaviour
    {
        public string EditableIdentifier;


        public void Initialise(Sphere inSphere)
        {
            var token = gameObject.GetComponent<Token>();
            var terrainFeaturePayload = new TerrainFeature();
            terrainFeaturePayload.SetId(EditableIdentifier);
            token.SetPayload(terrainFeaturePayload);
            
            inSphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));

            //A terrain feature will likely have permanent sphere children.
            var sphereComponentsInChildren = gameObject.GetComponentsInChildren<Sphere>();
            foreach (var s in sphereComponentsInChildren)
            {
                var spec = s.GetComponent<PermanentSphereSpec>();
                spec.ApplySpecToSphere(s);
                terrainFeaturePayload.AttachSphere(s);
            }
        }
    }
}
