#if MODS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    public static class HashtableExtensions
    {
        public static string GetStringOrLogError(this Hashtable hash, string key, HashSet<string> errors)
        {
            if (hash.ContainsKey(key))
            {
                if (hash[key] is string)
                {
                    return hash.GetString(key);
                }
                errors.Add("Invalid type for '" + key + "', should be string");
            }
            else
            {
                errors.Add("Missing '" + key + "'");
            }
            return null;
        }

        public static Hashtable DeepClone(this Hashtable a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (Hashtable) formatter.Deserialize(stream);
            }
        }
    }
}
#endif