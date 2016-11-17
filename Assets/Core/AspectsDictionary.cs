using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Assets.Core
{
    public interface IAspectsDictionary
    {
        int AspectValue(string aspectId);
        void Add(string key, int value);
        void Clear();
        bool ContainsKey(string key);
        bool ContainsValue(int value);
        void GetObjectData(SerializationInfo info, StreamingContext context);
        void OnDeserialization(object sender);
        bool Remove(string key);
        bool TryGetValue(string key, out int value);
        Dictionary<string, int>.Enumerator GetEnumerator();
        int Count { get; }
        IEqualityComparer<string> Comparer { get; }
        Dictionary<string, int>.KeyCollection Keys { get; }
        Dictionary<string, int>.ValueCollection Values { get; }
        int this[string key] { get; set; }
    }

    public class AspectsDictionary: Dictionary<string, int>, IAspectsDictionary
    {
        public int AspectValue(string aspectId)
        {
            if (ContainsKey(aspectId))
                return this[aspectId];

            return 0;
        }
    }
}
