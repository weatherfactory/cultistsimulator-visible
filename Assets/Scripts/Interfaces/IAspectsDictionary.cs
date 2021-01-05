using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Interfaces
{ 

public interface IAspectsDictionary : IDictionary<string, int>
{
    int AspectValue(string aspectId);
    List<string> KeysAsList();
    void CombineAspects(IAspectsDictionary additionalAspects);
    void ApplyMutations(Dictionary<string, int> mutations);

}

}
