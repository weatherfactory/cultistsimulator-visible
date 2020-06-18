using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   public class FucineEntityFactory
    {

        public IEntity CreateEntity(Type entityType)
        {
            return Activator.CreateInstance(entityType) as IEntity;
        }
    }
}
