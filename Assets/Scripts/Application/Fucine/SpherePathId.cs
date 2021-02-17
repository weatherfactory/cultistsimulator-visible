using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;

namespace Assets.Scripts.Application.Fucine
{

    public class SpherePathId: FucinePathId
    {
        private char sphereIdPrefix => FucinePath.SPHERE;


        public SpherePathId(string pathId) : base(pathId)
        {
            if (pathId.First() == sphereIdPrefix)
                _pathId = pathId;
            else
                _pathId = sphereIdPrefix + pathId;
        }

        public override PathCategory Category => PathCategory.Token;
    }
}
