using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Interfaces;

namespace SecretHistories.Fucine
{
    public class TokenPathId: FucinePathId
    {

private char tokenIdPrefix => FucinePath.SITUATION;

        public TokenPathId(string pathId) : base(pathId)
        {
            if (pathId.First() == tokenIdPrefix)
                _pathId = pathId;
            else
                _pathId = tokenIdPrefix + pathId;
        }

    }
}
