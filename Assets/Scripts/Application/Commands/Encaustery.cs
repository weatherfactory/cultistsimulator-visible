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
    public class Encaustery<T> where T: class,new()
    {
        public T Encaust(IEncaustable encaustable) 
        {
            throwExceptionIfEncaustmentAttributesAreHinky(encaustable,typeof(T));
            
            T encaustedCommand = setCommandPropertiesFromEncaustable<T>(encaustable);

            return encaustedCommand;

        }


        private void throwExceptionIfEncaustmentAttributesAreHinky(IEncaustable encaustable,Type specifiedGenericCommandType)
        {
            //is this an encaustable class?
            var isEncaustableClassAttribute = GetEncaustableClassAttribute(encaustable);

            if(isEncaustableClassAttribute==null)
                throw new ApplicationException($"{encaustable.GetType()} can't be encausted: it isn't marked with the IsEncaustableClass attribute");

            if(isEncaustableClassAttribute.ToType!=specifiedGenericCommandType)
                throw new ApplicationException($"{encaustable.GetType()} encausts to {isEncaustableClassAttribute.ToType}, but we're trying to encaust it to {specifiedGenericCommandType}");

            
            
            var encaustmentPropertyAttributeProblems = getEncaustmentPropertyAttributeProblems(encaustable, specifiedGenericCommandType);

            if (encaustmentPropertyAttributeProblems.Any())
                throw new ApplicationException(string.Join("\n",encaustmentPropertyAttributeProblems));

        }

        private List<string> getEncaustmentPropertyAttributeProblems(IEncaustable encaustable, Type specifiedGenericCommandType)
        {
            List<string> encaustmentPropertyAttributeProblems = new List<string>();

            var allPropertiesOnEncaustable = encaustable.GetType().GetProperties();
            foreach (var p in allPropertiesOnEncaustable)
            {
                //are all properties marked either encaust or dontencaust?
                if (!p.IsDefined(typeof(Encaust), false) && !p.IsDefined(typeof(DontEncaust), false))
                    encaustmentPropertyAttributeProblems.Add(
                        $"Trying to encaust type {encaustable}: property {p.Name} isn't marked as either Encaust or DontEncaust");

                //is there a matching property of the same name on the command that we're encausting to?
                if (p.IsDefined(typeof(Encaust)) && specifiedGenericCommandType.GetProperty(p.Name) == null)
                    encaustmentPropertyAttributeProblems.Add(
                        $"Encaustable of type {encaustable.GetType()} has Encaust-marked property {p.Name}, but the command we're encausting to ({specifiedGenericCommandType}) doesn't have a property with a matching name.");
            }

            foreach (var cp in specifiedGenericCommandType.GetProperties())
            {
                var candidateEncaustableProperty = allPropertiesOnEncaustable.SingleOrDefault(p => p.Name == cp.Name);
                if (candidateEncaustableProperty == null)
                    throw new ApplicationException(
                        $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {encaustable.GetType()} lacks, so we can't encaust the other to the one.");
                if (candidateEncaustableProperty.IsDefined(typeof(DontEncaust)))
                    throw new ApplicationException(
                        $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {encaustable.GetType()} marks as DontEncaust, so we can't encaust the other to the one.");
            }

            return encaustmentPropertyAttributeProblems;
        }

        private IsEncaustableClass GetEncaustableClassAttribute(IEncaustable encaustable)
        {
            IsEncaustableClass isEncaustableClassAttribute;
            isEncaustableClassAttribute =
                encaustable.GetType().GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as
                    IsEncaustableClass;
            return isEncaustableClassAttribute;
        }

        private List<PropertyInfo> GetEncaustablePropertiesForType(Type t)
        {
            var allPropertiesOnEncaustable = t.GetProperties();
            List<PropertyInfo> encaustableProperties = new List<PropertyInfo>();

            foreach (var encaustP in allPropertiesOnEncaustable)
            {
                if (encaustP.IsDefined(typeof(Encaust), false))
                {
                    encaustableProperties.Add(encaustP);
                }

            }

            return encaustableProperties;
        }

        private T setCommandPropertiesFromEncaustable<T>(IEncaustable encaustable) where T : class, new()
        {
            var command=new T();

            foreach (var encaustableProperty in GetEncaustablePropertiesForType(encaustable.GetType()))
            {
                var commandPropertyToSet = typeof(T).GetProperty(encaustableProperty.Name);

                if (typeof(IEncaustable).IsAssignableFrom(encaustableProperty.PropertyType))
                {
                    object innerEncaustedCommand = encaustPropertyAsCommandInItsOwnRight(encaustable, encaustableProperty);
                        commandPropertyToSet.SetValue(command, innerEncaustedCommand);
                }

                else
                    commandPropertyToSet.SetValue(command, encaustableProperty.GetValue(encaustable));



            }

            return command;
        }

        private object encaustPropertyAsCommandInItsOwnRight(IEncaustable outerEncaustable, PropertyInfo innerEncaustableAsProperty)
        {
            IEncaustable innerEncaustableInstance = innerEncaustableAsProperty.GetValue(outerEncaustable) as IEncaustable;
            Type encaustInnerAsType = GetEncaustableClassAttribute(innerEncaustableInstance).ToType;
            Type baseEncausteryType = typeof(Encaustery<>);
            Type combinedTypeWithGenericArgument = baseEncausteryType.MakeGenericType(encaustInnerAsType);

            object innerEncaustery = Activator.CreateInstance(combinedTypeWithGenericArgument);
            var encaustMethod = innerEncaustery.GetType().GetMethod("Encaust");
          var encaustedInnerCommand= encaustMethod.Invoke(innerEncaustery, new object[]{innerEncaustableInstance});

          return encaustedInnerCommand;

        }
    }
}
