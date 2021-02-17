using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;

namespace Assets.Scripts.Application.Fucine
{

    public class SpherePathPart: FucinePathPart
    {
        private char sphereIdPrefix => FucinePath.SPHERE;


        public SpherePathPart(string pathId) : base(pathId)
        {
            if (pathId.First() == sphereIdPrefix)
                PathId = pathId;
            else
                PathId = sphereIdPrefix + pathId;
        }

        public override PathCategory Category => PathCategory.Token;
    }
}
