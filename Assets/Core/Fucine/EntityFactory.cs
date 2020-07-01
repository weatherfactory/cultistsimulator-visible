using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Fucine
{

    public interface IFactory
    {
        object ConstructorFastInvoke();
    }
    
    public class EntityFactory<T> where T: AbstractEntity<T>
    {

        private readonly Func<Hashtable,ContentImportLog,T> FastInvokeConstructor;


        public EntityFactory()
        {

            FastInvokeConstructor = FastInvoke.BuildEntityConstructor<T>();
        }

        public T ConstructorFastInvoke(Hashtable data, ContentImportLog log)
        {
           return FastInvokeConstructor(data,log);
        }

    }


    public class NonEntityFactory<T>:IFactory
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
