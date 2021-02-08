using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Infrastructure.Persistence;

namespace SecretHistories.Commands.Encausting
{
   public class SerializationHelper
    {
        private JsonSerializer serializer;
        
            public SerializationHelper()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            // jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            serializer = JsonSerializer.Create(jsonSerializerSettings);
        }

            public string SerializeToJsonString(IEncaustment encaustment)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                JsonWriter jw = new JsonTextWriter(sw);
                jw.Formatting = Formatting.Indented;

                serializer.Serialize(jw, encaustment);

                string json = sb.ToString();

                return json;
            }

            public T DeserializeFromJsonString<T>(string json) where T:IEncaustment
            {
             var encaustment=JsonConvert.DeserializeObject<T>(json);
             return encaustment;
            }

    }
}
