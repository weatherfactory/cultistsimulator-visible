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

        public TokenPathPart(string pathpartvalue) : base(pathpartvalue)
        {
            if (pathpartvalue.First() == tokenIdPrefix)
                PathPartValue = pathpartvalue;
            else
                PathPartValue = tokenIdPrefix + pathpartvalue;
        }


        public override PathCategory Category => PathCategory.Token;
        public override string GetId()
        {
            if (PathPartValue.First() == tokenIdPrefix)
                return PathPartValue.Substring(1);

            throw new ApplicationException("Can't find the token ID in token pathpart " + PathPartValue);
        }
    }
}
