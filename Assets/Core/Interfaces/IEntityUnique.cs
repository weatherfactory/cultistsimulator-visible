using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;

namespace Assets.Core.Interfaces
{
    /// <summary>
    /// This is unique *in context*, not necessarily across the whole game. e.g.: a particular
    /// </summary>
    interface  IEntityUnique:IEntity
    {
        string Id { get; }
        void SetId(string id);
    }
}
