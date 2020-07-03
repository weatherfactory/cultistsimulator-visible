using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public interface IEntityFactory
    {
        object ConstructorFastInvoke(EntityData data, ContentImportLog log);

    }
    
    public class EntityFactory<T>: IEntityFactory where T: AbstractEntity<T>
    {

        private readonly Func<EntityData,ContentImportLog,T> _fastInvokeConstructor;


        public EntityFactory()
        {

            _fastInvokeConstructor = PrecompiledInvoke.BuildEntityConstructor<T>();
        }

        public object ConstructorFastInvoke(EntityData data, ContentImportLog log)
        {
           return _fastInvokeConstructor(data,log);
        }

    }

    public interface INonEntityFactory
    {
        object ConstructorFastInvoke();
    }
}
