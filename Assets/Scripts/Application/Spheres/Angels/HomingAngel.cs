using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace SecretHistories.Spheres.Angels
{
    public class HomingAngel : IAngel
    {
        public Vector3 PreferredHomingPosition;
        public DateTime WhenHomingSet;
        public Token What;
        private Sphere sphereToWatchOver;

        public HomingAngel(Token what)
        {
            PreferredHomingPosition = what.TokenRectTransform.anchoredPosition3D;
            WhenHomingSet = DateTime.Now;
            What = what;
        }

        public void Act(float interval)
        {
            //could put something here to remove this angel from the watched sphere if it's got stale
        }

        public void SetWatch(Sphere sphere)
        {
            sphereToWatchOver = sphere;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            if (token == What)
            {
                var destination = sphereToWatchOver.Choreographer.GetFreeLocalPosition(token, PreferredHomingPosition);
                TokenTravelItinerary travellingHome =
                    new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, destination);
                
                travellingHome.Depart(token);
                
                sphereToWatchOver.RemoveAngel(this);
                return true;
            }

            return false;
        }
    }
}
