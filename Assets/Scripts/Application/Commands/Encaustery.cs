using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            throwExceptionIfEncaustmentAttributesAreHinky(encaustable,typeof(T));
            
            

            T encaustedCommand = setCommandPropertiesFromEncaustable<T>(encaustable);


            return encaustedCommand;

        }



        private void throwExceptionIfEncaustmentAttributesAreHinky(IEncaustable encaustable,Type specifiedGenericCommandType)
        {
            //is this an encaustable class?
            IsEncaustableClass isEncaustableClassAttribute = encaustable.GetType().GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as IsEncaustableClass;
            if(isEncaustableClassAttribute==null)
                throw new ApplicationException($"{encaustable.GetType()} can't be encausted: it isn't marked with the IsEncaustableClass attribute");

            //are we encausting to the expected type?
            if(isEncaustableClassAttribute.ToType!=specifiedGenericCommandType)
                throw new ApplicationException($"{encaustable.GetType()} encausts to {isEncaustableClassAttribute.ToType}, but we're trying to encaust it to {specifiedGenericCommandType}");


            
            var allPropertiesOnEncaustable = encaustable.GetType().GetProperties();
            List<string> encaustmentPropertyAttributeProblems=new List<string>();

            
            foreach (var p in allPropertiesOnEncaustable)
            {
                //are all properties marked either encaust or dontencaust?
                if (!p.IsDefined(typeof(Encaust), false) && !p.IsDefined(typeof(DontEncaust),false))
                    encaustmentPropertyAttributeProblems.Add($"Trying to encaust type {encaustable}: property {p.Name} isn't marked as either Encaust or DontEncaust");   

                //is there a matching property of the same name on the command that we're encausting to?
                if(p.IsDefined(typeof(Encaust)) && specifiedGenericCommandType.GetProperty(p.Name)==null)
                    encaustmentPropertyAttributeProblems.Add($"Encaustable of type {encaustable.GetType()} has Encaust-marked property {p.Name}, but the command we're encausting to ({specifiedGenericCommandType}) doesn't have a property with a matching name.");
            }

            if (encaustmentPropertyAttributeProblems.Any())
            {
                throw new ApplicationException(string.Join("\n",encaustmentPropertyAttributeProblems));
            }

        }

        private T setCommandPropertiesFromEncaustable<T>(IEncaustable encaustable) where T : class, new()
        {
            var command=new T();

            
            foreach (var encaustP in GetEncaustablePropertiesForType(encaustable.GetType()))
            {
                var commandPropertyToSet = typeof(T).GetProperty(encaustP.Name);
                    commandPropertyToSet.SetValue(command, encaustP.GetValue(encaustable));
            }

            return command;
        }

        private List<PropertyInfo> GetEncaustablePropertiesForType(Type t)
        {
            var allPropertiesOnEncaustable = t.GetProperties();
            List<PropertyInfo> encaustableProperties=new List<PropertyInfo>();

            foreach (var encaustP in allPropertiesOnEncaustable)
            {
                if (encaustP.IsDefined(typeof(Encaust), false))
                {
                    encaustableProperties.Add(encaustP);
                }

            }

            return encaustableProperties;
        }
    }
}
