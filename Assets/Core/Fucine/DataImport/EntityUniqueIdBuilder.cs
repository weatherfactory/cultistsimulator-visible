using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Noon;

namespace Assets.Core.Fucine.DataImport
{
    public class EntityUniqueIdBuilder
    {
        private readonly StringBuilder _uniqueId;
        public string UniqueId => _uniqueId.ToString();


        public EntityUniqueIdBuilder(JToken forToken)
        {
           _uniqueId=new StringBuilder(BuildIdForToken(forToken));

        }


        public EntityUniqueIdBuilder(JToken forToken, EntityUniqueIdBuilder soFar)
        {
            _uniqueId = new StringBuilder(soFar.UniqueId);
            _uniqueId.Append(BuildIdForToken(forToken));

        }

        private string BuildIdForToken(JToken forToken)
        {
            string buildingId = string.Empty; 

            //if this token is a value, use the key from its immediate ancestor
            if (forToken is JProperty propertyToken)
            {
                buildingId=$".{propertyToken.Name}";
            }

            else if (forToken is JObject objectToken)
            {
                //if this token has an id, use that with ">" notation
                var tokenId = objectToken[NoonConstants.ID];
                if (tokenId?.Type == JTokenType.String)
                {
                    buildingId = $">[{tokenId}]";
                }
            }



            return buildingId;
        }


    }
}
