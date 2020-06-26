using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   public class FucineEntityFactory<T> where T:AbstractEntity<T>
    {

        public AbstractEntity<T> CreateEntity(Type entityType)
        {
            if (!(Activator.CreateInstance(entityType) is AbstractEntity<T> createdEntity))
             throw new ApplicationException(
                 $"Couldn't create a Fucine AbstractEntity of type {entityType}. (Does this type implement AbstractEntity?)");

            return createdEntity;
        }
    }
}
