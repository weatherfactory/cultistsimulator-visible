using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;

namespace Assets.Core.Interfaces
{
    interface  IEntityUnique:IEntity
    {
        string Id { get; }
        void SetId(string id);
    }
}
