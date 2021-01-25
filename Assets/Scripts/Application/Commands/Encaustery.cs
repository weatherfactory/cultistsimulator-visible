using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;

namespace SecretHistories.Commands
{
    public class Encaustery
    {
        public T EncaustTo<T>(IEncaustable encaustable) where T: class
        {
            checkEncaustableAttributes(encaustable);

            return encaustable as T;

        }

        private void checkEncaustableAttributes(IEncaustable encaustable)
        {
            var attributes = encaustable.GetType().GetCustomAttributes(typeof(EncaustableClass), false);
            if(!attributes.Any())
                throw new ApplicationException($"{encaustable.GetType()} isn't marked with the EncaustableClass attribute");
        }
    }
}
