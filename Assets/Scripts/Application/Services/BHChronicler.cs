using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Services
{
    public class BHChronicler: AbstractChronicler

    {
        public override void TokenPlacedInWorld(Token token)
        {
            //
        }

        public override void ChronicleGameEnd(List<Situation> situations, HashSet<Sphere> tokenContainers, Ending ending)
        {
           //
        }

        public override void SetAchievementsForEnding(Ending ending)
        {
            throw new NotImplementedException();
        }

        public override void ChronicleSpecificsForElementStacksAtGameEnd(List<Token> elementTokens)
        {
        //
        }


        public override void ChronicleOtherworldEntry(string portal)
        {
          //
        }
    }
}
