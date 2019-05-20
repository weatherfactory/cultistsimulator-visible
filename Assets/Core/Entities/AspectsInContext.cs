using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class AspectsInContext
    {
        public readonly IAspectsDictionary _aspectsInSituation;
        public readonly IAspectsDictionary _aspectsOnTable;
        public readonly IAspectsDictionary _aspectsExtant;

        public AspectsInContext(IAspectsDictionary aspectsInSituation, IAspectsDictionary aspectsOnTable, IAspectsDictionary aspectsExtant)
        {
            _aspectsInSituation = aspectsInSituation;
            _aspectsOnTable = aspectsOnTable;
            _aspectsExtant = aspectsExtant;
        }

        public IAspectsDictionary AspectsExtant
        {
            get { return _aspectsExtant; }
        }

        public IAspectsDictionary AspectsOnTable
        {
            get { return _aspectsOnTable; }
        }

        public IAspectsDictionary AspectsInSituation
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
