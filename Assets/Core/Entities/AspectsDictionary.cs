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

        void CombineAspects(IAspectsDictionary additionalAspects);
    }

    public class AspectsDictionary: Dictionary<string, int>, IAspectsDictionary
    {
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
    }
}
