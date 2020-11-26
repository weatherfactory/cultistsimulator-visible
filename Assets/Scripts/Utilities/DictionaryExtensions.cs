using System.Collections.Generic;
using System.Linq;

namespace Noon
{
    public static class DictionaryExtensions
    {
        public static bool IsEquivalentTo<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
        {
            return dictionary.Keys.Count == otherDictionary.Keys.Count &&
                   dictionary.Keys.All(
                       k => otherDictionary.ContainsKey(k) && Equals(otherDictionary[k], dictionary[k]));
        }
    }
}