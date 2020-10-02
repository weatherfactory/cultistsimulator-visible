using System;
using Noon;

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
            try
            {
                return _fastInvokeConstructor();
            }
            catch (Exception)
            {
                NoonUtility.Log($"Can't use a fastinvokeconstructor. This might mean a FucineSubEntity is missing a parameterless constructor.",2,VerbosityLevel.Essential);
                throw;
            }
        }

    }
}