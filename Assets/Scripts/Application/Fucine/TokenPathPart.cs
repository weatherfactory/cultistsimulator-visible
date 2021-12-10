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
            //if (PathPartValue.First() == tokenIdPrefix)
            //    return PathPartValue.Substring(1);

            //no: we're keeping the tokenidprefix in the id, so tokenids are always immediately identifiable
            if (PathPartValue.First() == tokenIdPrefix)
                return PathPartValue;

            throw new ApplicationException("Can't find the token ID in token pathpart " + PathPartValue);
        }
    }
}
