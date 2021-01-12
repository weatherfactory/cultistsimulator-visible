﻿using System;
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
        protected Token OriginToken;
        protected Vector3 PreferredHomingPosition;
        protected DateTime WhenHomingSet;
        protected Token TokenToBringHome;
        protected Sphere SphereToWatchOver;

        public HomingAngel(Token tokenToBringHome)
        {
            PreferredHomingPosition = tokenToBringHome.TokenRectTransform.anchoredPosition3D;
            WhenHomingSet = DateTime.Now;
            TokenToBringHome = tokenToBringHome;
        }

        public int Authority => 10;

        public void Act(float interval)
        {
            //could put something here to remove this angel from the watched sphere if it's got stale
        }

        public void SetWatch(Sphere sphere)
        {
            SphereToWatchOver = sphere;
        }

        public void SetOriginToken(Token originToken)
        {
            OriginToken = originToken;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            if (token == TokenToBringHome)
            {
                if (OriginToken != null)
                    ReturnToOriginTokenLocation(context);
                else 
                    ReturnToHomeLocation(context);

                SphereToWatchOver.RemoveAngel(this);
                return true;
            }

            return false;
        }

        private void ReturnToOriginTokenLocation(Context context)
        {
            var destination = OriginToken.TokenRectTransform.anchoredPosition3D;
            SendToken(TokenToBringHome, destination, context);
        }

        private void ReturnToHomeLocation(Context context)
        {
            var destination = SphereToWatchOver.Choreographer.GetFreeLocalPosition(TokenToBringHome, PreferredHomingPosition);
            SendToken(TokenToBringHome,destination, context);
        }

        private void SendToken(Token token, Vector3 destination,Context context)
        {
            TokenTravelItinerary travellingHome =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, destination)
                    .WithDuration(NoonConstants.MOMENT_TIME_INTERVAL);

            travellingHome.Depart(token,context);
        }
    }
}