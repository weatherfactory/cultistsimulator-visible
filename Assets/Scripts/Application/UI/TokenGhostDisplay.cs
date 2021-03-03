using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{


 public class TokenGhostDisplay: MonoBehaviour
    {

        private Dictionary<string,IGhost> ghosts=new Dictionary<string,IGhost>();

        public void DisplayGhost(Token forToken,Sphere inSphere)
        {
            IGhost ghost;
            if (!ghosts.ContainsKey(forToken.PayloadId))
            {
                ghost = Watchman.Get<PrefabFactory>()
                    .CreateGhostPrefab(forToken.GetGhostManifestationType(), this.gameObject.transform);
                ghosts[forToken.PayloadId] = ghost;
            }
            else
                ghost = ghosts[forToken.PayloadId];

            var candidatePosition=inSphere.Choreographer.GetFreeLocalPosition(forToken, forToken.Location.Anchored3DPosition);
            
            ghost.ShowPosition(candidatePosition);

        }
    }
}
