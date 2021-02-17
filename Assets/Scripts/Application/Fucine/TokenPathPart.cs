using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class TokenPathPart: FucinePathPart
    {

private char tokenIdPrefix => FucinePath.TOKEN;

        public TokenPathPart(string pathId) : base(pathId)
        {
            if (pathId.First() == tokenIdPrefix)
                PathId = pathId;
            else
                PathId = tokenIdPrefix + pathId;
        }


        public TokenPathPart  FromVerbId(string verbId)
        {
            return new TokenPathPart(verbId + Guid.NewGuid());
            
        }

        public override PathCategory Category => PathCategory.Token;

    }
}
