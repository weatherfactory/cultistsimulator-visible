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

        public Entity CreateEntity(Type entityType)
        {
            if (!(Activator.CreateInstance(entityType) is Entity createdEntity))
             throw new ApplicationException(
                 $"Couldn't create a Fucine IEntity of type {entityType}. (Does this type implement IEntity?)");

            return createdEntity;
        }
    }
}
