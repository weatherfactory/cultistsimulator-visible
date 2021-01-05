using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using SecretHistories.UI;
using SecretHistories.Interfaces;

namespace SecretHistories.Core
{

    public class AspectsDictionary: Dictionary<string, int>, IAspectsDictionary
    {
        public AspectsDictionary():this(new Dictionary<string, int>())
        { }

        public static AspectsDictionary GetFromStacks(IEnumerable<ElementStack> stacks,bool includingSelf=true)
        {
            AspectsDictionary totals = new AspectsDictionary();

            foreach (var elementCard in stacks)
            {
                var aspects = elementCard.GetAspects(includingSelf);

                foreach (string k in aspects.Keys)
                {
                    if (totals.ContainsKey(k))
                        totals[k] += aspects[k];
                    else
                        totals.Add(k, aspects[k]);
                }
            }

            return totals;

        }


        public AspectsDictionary(Dictionary<string, int> aspectsAsDictionary)
        {
            foreach(var kvp in aspectsAsDictionary)
                Add(kvp.Key,kvp.Value);
        }

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
