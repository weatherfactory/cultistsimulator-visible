using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;

namespace Assets.Core.Interfaces
{
    public interface IEntity
    {

        /// <summary>
        /// This is run for every entity when the compendium has been completely (re)populated. Use for entities that
        /// need additional initialisation based on data from other entities
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="populatedCompendium"></param>
        void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium);
    }
}
