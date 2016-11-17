using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Assets.Core
{
    public interface IAspectsDictionary: IDictionary<string,int>
    {
        int AspectValue(string aspectId);

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
