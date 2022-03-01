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
            var customBinder = new EncaustablesSerializationBinder();
            var jsonSerializerSettings = new JsonSerializerSettings();
      //       jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
     //   jsonSerializerSettings.Binder = customBinder;
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
                //var encaustment=JsonConvert.DeserializeObject<T>(json);

             StringReader sr=new StringReader(json);
             JsonReader jr=new JsonTextReader(sr);
             var encaustment= serializer.Deserialize<T>(jr);


             return encaustment;
            }

            public bool MightBeJson(string possiblyJson)
            {
                if (possiblyJson.IndexOf('{') > -1)
                    return true;

                return false;

            }

    }
}
