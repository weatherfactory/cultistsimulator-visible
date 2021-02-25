using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace Assets.Scripts.Application.Fucine
{

    public class SpherePathPart: FucinePathPart
    {
        private char sphereIdPrefix => FucinePath.SPHERE;

        public override PathCategory Category => PathCategory.Sphere;
        public override string GetId()
        {
            if (PathPartValue.First() == sphereIdPrefix)
                return PathPartValue.Substring(1);

            throw new ApplicationException("Can't find the sphere ID in sphere Ipathpart " + PathPartValue);

        }

        public SpherePathPart(string pathPartValue) : base(pathPartValue)
        {
            if (pathPartValue.First() == sphereIdPrefix)
                PathPartValue = pathPartValue;
            else
                PathPartValue = sphereIdPrefix + pathPartValue;
        }


        
    }
}
