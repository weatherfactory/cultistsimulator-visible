using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;

namespace Assets.Scripts.Application.Commands.Encausting
{
   public class JSONPortal<T> where T: IEncaustment
    {
        public string Serialize(T encaustment)
        {
            string output = JsonConvert.SerializeObject(encaustment);
            return output;
        }

        public T Deserialize(string fromJson)
        {
            T deserialized = JsonConvert.DeserializeObject<T>(fromJson);
            return deserialized;
        }

    }
}
