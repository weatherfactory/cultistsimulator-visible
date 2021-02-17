using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Fucine;

namespace SecretHistories.Entities
{
    public class AspectsInContext
    {
        public readonly AspectsDictionary _aspectsInSituation;
        public readonly AspectsDictionary _aspectsOnTable;
        public readonly AspectsDictionary _aspectsExtant;

        public AspectsInContext(AspectsDictionary aspectsInSituation, AspectsDictionary aspectsOnTable, AspectsDictionary aspectsExtant)
        {
            _aspectsInSituation = aspectsInSituation;
            _aspectsOnTable = aspectsOnTable;
            _aspectsExtant = aspectsExtant;
        }

        public AspectsDictionary AspectsExtant
        {
            get { return _aspectsExtant; }
        }

        public AspectsDictionary AspectsOnTable
        {
            get { return _aspectsOnTable; }
        }

        public AspectsDictionary AspectsInSituation
        {
            get { return _aspectsInSituation; }
        }

        public  void ThrowErrorIfNotPopulated(string verbId)
        {
            if (AspectsInSituation == null)
                throw new ApplicationException("Unpopulated Aspects in Situation passed to " + verbId);
            
            if (_aspectsOnTable == null)
                throw new ApplicationException("Unpopulated Aspects on Table passed to " + verbId);

            if (AspectsExtant == null)
                throw new ApplicationException("Unpopulated Aspects Extant passed to " + verbId);
        }
    }
}
