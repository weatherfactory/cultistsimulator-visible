using System;

namespace Assets.Core.Fucine
{
    public interface IFastInvokableObjectFactory
    {
        object ConstructorFastInvoke();
    }


    public class FastInvokableObjectFactory<T>:IFastInvokableObjectFactory
    {

        private readonly Func<T> _fastInvokeConstructor;


        public FastInvokableObjectFactory()
        {

            _fastInvokeConstructor = PrecompiledInvoke.BuildDefaultConstructor<T>();
        }

        public object ConstructorFastInvoke()
        {
            return _fastInvokeConstructor();
        }

    }
}