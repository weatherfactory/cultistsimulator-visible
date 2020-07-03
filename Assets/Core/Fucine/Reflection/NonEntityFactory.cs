using System;

namespace Assets.Core.Fucine
{
    public class NonEntityFactory<T>:INonEntityFactory
    {

        private readonly Func<T> _fastInvokeConstructor;


        public NonEntityFactory()
        {

            _fastInvokeConstructor = PrecompiledInvoke.BuildDefaultConstructor<T>();
        }

        public object ConstructorFastInvoke()
        {
            return _fastInvokeConstructor();
        }

    }
}