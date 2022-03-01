using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SecretHistories.Abstract;
using SecretHistories.Spheres;


namespace SecretHistories.Commands.Encausting
{
    public class EncaustablesSerializationBinder: SerializationBinder
    {
        public IList<Type> EncaustableTypes;
        //proof of concept works; I'll revisit when I'm not about to hit live

        public EncaustablesSerializationBinder()
        {
            EncaustableTypes = new List<Type>();
            EncaustableTypes.Add(typeof(TokenCreationCommand));
            EncaustableTypes.Add(typeof(DropzoneCreationCommand));
            EncaustableTypes.Add(typeof(BubbleSphere));
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            int beginAt = typeName.LastIndexOf('.') + 1;
            string shortName = typeName.Substring(beginAt);

            return EncaustableTypes.SingleOrDefault(t => t.Name == shortName);
        }


    }
}
