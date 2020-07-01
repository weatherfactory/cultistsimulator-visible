using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public interface IEntityFactory
    {
        object ConstructorFastInvoke(Hashtable data, ContentImportLog log);

    }
    
    
    public class EntityFactory<T>: IEntityFactory where T: AbstractEntity<T>
    {

        private readonly Func<Hashtable,ContentImportLog,T> FastInvokeConstructor;


        public EntityFactory()
        {

            FastInvokeConstructor = FastInvoke.BuildEntityConstructor<T>();
        }

        public object ConstructorFastInvoke(Hashtable data, ContentImportLog log)
        {
           return FastInvokeConstructor(data,log);
        }

    }

    public interface INonEntityFactory
    {
        object ConstructorFastInvoke();
    }


    public class NonEntityFactory<T>:INonEntityFactory
    {

        private readonly Func<T> FastInvokeConstructor;


        public NonEntityFactory()
        {

            FastInvokeConstructor = FastInvoke.BuildDefaultConstructor<T>();
        }

        public object ConstructorFastInvoke()
        {
            return FastInvokeConstructor();
        }

    }
}
