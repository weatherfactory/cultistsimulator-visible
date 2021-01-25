using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;

namespace SecretHistories.Commands
{
    public class Encaustery
    {
        public T EncaustTo<T>(IEncaustable encaustable) where T: class,new()
        {
            checkEncaustableAttributes(encaustable,typeof(T));
            


            T encaustedCommand=new T();

            return encaustedCommand;

        }

        private void checkEncaustableAttributes(IEncaustable encaustable,Type specifiedGenericType)
        {
            IsEncaustableClass isEncaustableClassAttribute = encaustable.GetType().GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as IsEncaustableClass;
            if(isEncaustableClassAttribute==null)
                throw new ApplicationException($"{encaustable.GetType()} can't be encausted: it isn't marked with the IsEncaustableClass attribute");
            if(isEncaustableClassAttribute.ToType!=specifiedGenericType)
                throw new ApplicationException($"{encaustable.GetType()} encausts to {isEncaustableClassAttribute.ToType}, but we're trying to encaust it to {specifiedGenericType}");


        }
    }
}
