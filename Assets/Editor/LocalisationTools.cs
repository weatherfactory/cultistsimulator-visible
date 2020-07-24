using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noon;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class LocalisationTools
    {
       
        private static readonly string[] ValidFields =
        {
            "id",
            "label",
            "description",
            "startdescription",
            "drawmessages",
            "slots",
            "linked",
            "alt",
            "alternativerecipes"
        };

      

        private static ArrayList PruneArrayList(IEnumerable source)
        {
            return new ArrayList(source.Cast<Hashtable>().Select(PruneHashtable).ToList());
        }

        private static Entity PruneHashtable(Hashtable source)
        {
            var newEntity = new Entity();
            foreach (string property in source.Keys.Cast<string>().Where(p => ValidFields.Contains(p)).ToList())
            {
                switch (property)
                {
                    case "slots":
                    case "linked":
                    case "alt":
                    case "alternativerecipes":
                        var newList = PruneArrayList((ArrayList) source[property]);
                        var onlyIds = newList.Cast<Entity>().All(entry => entry.IsIdOnly());

                        if (!onlyIds)
                        {
                            SetEntityProperty(newEntity, property, newList);
                        }
                        break;
                    default:
                        SetEntityProperty(newEntity, property, source[property]);
                        break;
                }
            }
            return newEntity;
        }

        private static void SetEntityProperty(Entity entity, string name, object value)
        {
            typeof(Entity).GetProperty(name).SetValue(entity, value);
        }

        private static void WriteJsonToFile(string filePath, object data)
        {
            using (var file = File.OpenWrite(filePath))
            {
                using (var writer = new StreamWriter(file))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        jsonWriter.Formatting = Formatting.Indented;
                        jsonWriter.Indentation = ' ';
                        jsonWriter.Indentation = 4;
                        JsonSerializer serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
                        serializer.Serialize(jsonWriter, data);
                    }
                }
            }
        }

        private static void Write(TextWriter writer, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                writer.WriteLine(text.Replace("\n", @"\n"));
            }
        }

        private static void Write(TextWriter writer, IEnumerable<string> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    Write(writer, text);
                }
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class Entity
        {
            [JsonProperty(Order = 1)]
            public string id { get; set; }

            [JsonProperty(Order = 2)]
            public string label { get; set; }

            [JsonProperty(Order = 3)]
            public string description { get; set; }

            [JsonProperty(Order = 4)]
            public string startdescription { get; set; }

            [JsonProperty(Order = 5)]
            public Hashtable drawmessages { get; set; }

            [JsonProperty(Order = 6)]
            public ArrayList slots { get; set; }

            [JsonProperty(Order = 7)]
            public ArrayList linked { get; set; }

            [JsonProperty(Order = 8)]
            public ArrayList alt { get; set; }

            [JsonProperty(Order = 9)]
            public ArrayList alternativerecipes { get; set; }

            public bool IsIdOnly()
            {
                return
                    label == null
                    && description == null
                    && startdescription == null
                    && drawmessages == null
                    && slots == null
                    && linked == null
                    && alt == null
                    && alternativerecipes == null;
            }
        }
    }
}
