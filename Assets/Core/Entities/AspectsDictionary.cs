using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Assets.Core
{
    public interface IAspectsDictionary: IDictionary<string,int>
    {
        int AspectValue(string aspectId);
        List<string> KeysAsList();
        void CombineAspects(IAspectsDictionary additionalAspects);
        void ApplyMutations(Dictionary<string, int> mutations);
    }

    public class AspectsDictionary: Dictionary<string, int>, IAspectsDictionary
    {
        public List<string> KeysAsList()
        {
            return Keys.ToList();
        }

        public int AspectValue(string aspectId)
        {
            if (ContainsKey(aspectId))
                return this[aspectId];

            return 0;
        }

        public void CombineAspects(IAspectsDictionary additionalAspects)
        {
            foreach (string k in additionalAspects.Keys)
            {
                if (this.ContainsKey(k))
                    this[k] += additionalAspects[k];
                else
                    Add(k, additionalAspects[k]);
            }
        }

        public void ApplyMutations(Dictionary<string, int> mutations)
        {
            foreach (KeyValuePair<string, int> mutation in mutations)
            {
                if (mutation.Value > 0)
                {
                    if (ContainsKey(mutation.Key))
                        this[mutation.Key] += mutation.Value;
                    else
                        Add(mutation.Key, mutation.Value);
                }
                else if (mutation.Value < 0)
                {
                    if (ContainsKey(mutation.Key))
                    {
                        if (AspectValue(mutation.Key) + mutation.Value <= 0)
                            Remove(mutation.Key);
                        else
                            this[mutation.Key] += mutation.Value;
                    }
                    else
                    {
                        //do nothing. We used to log this, but it's an issue when we are eg adding a -1 to remove an element that was added in play.
                        // NoonUtility.Log("Tried to mutate an aspect (" + mutation.Key + ") off an element (" + this._element.Id + ") but the aspect wasn't there.");
                    }
                }
            }
        }
    }
}
