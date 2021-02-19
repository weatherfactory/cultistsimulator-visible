using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using UnityEngine;

namespace SecretHistories.Commands
{
    public class Encaustery<T> where T: class, IEncaustment, new()
    {
        private Type CachedEncaustableType;

        public T Encaust(IEncaustable encaustable)
        {
            Type actualEncaustableType = GetActualEncaustableType(encaustable);

            //cache on the first run through the encaustery; it might have been reused (and will start as null), so we check and overwrite
            CachedEncaustableType = actualEncaustableType;

         ThrowExceptionIfEncaustmentAttributesAreHinky(encaustable,typeof(T));
            
         T encaustedCommand = setCommandPropertiesFromEncaustable<T>(encaustable);

         return encaustedCommand;

        }

        private Type GetActualEncaustableType(IEncaustable encaustable)
        {
            var encaustableAttribute = GetEncaustableClassAttributeFromEncaustableInstance(encaustable);
            if (encaustableAttribute != null)
                return encaustable.GetType();

            var emulousAttribute = GetEmulousEncaustableAttributeFromInstance(encaustable);
            if (emulousAttribute != null)
                return emulousAttribute.EmulateBaseEncaustableType;

            throw new ApplicationException($"trying to encaust a type ({encaustable.GetType()}) which isn't marked as either encaustable or emulous");
        }


        private void ThrowExceptionIfEncaustmentAttributesAreHinky(IEncaustable encaustable,Type specifiedGenericCommandType)
        {
            //is this an encaustable class?g
            var isEncaustableClassAttribute = GetEncaustableClassAttributeFromEncaustableInstance(encaustable);

            if(isEncaustableClassAttribute==null)
                throw new ApplicationException($"{CachedEncaustableType} can't be encausted: it isn't marked with the IsEncaustableClass attribute");

            if(isEncaustableClassAttribute.ToType!=specifiedGenericCommandType)
                throw new ApplicationException($"{CachedEncaustableType} encausts to {isEncaustableClassAttribute.ToType}, but we're trying to encaust it to {specifiedGenericCommandType}");

            
            
            var encaustmentPropertyAttributeProblems = getEncaustmentPropertyAttributeProblems(encaustable, specifiedGenericCommandType);

            if (encaustmentPropertyAttributeProblems.Any())
                throw new ApplicationException(string.Join("\n",encaustmentPropertyAttributeProblems));

        }

        private List<string> getEncaustmentPropertyAttributeProblems(IEncaustable encaustable, Type specifiedGenericCommandType)
        {
            List<string> encaustmentPropertyAttributeProblems = new List<string>();

            var propertiesToEncaust = GetPropertiesToEncaust(CachedEncaustableType);
            foreach (var p in propertiesToEncaust)
            {
                //are all properties marked either encaust or dontencaust?
                if (!p.IsDefined(typeof(Encaust), false) && !p.IsDefined(typeof(DontEncaust), false))
                    encaustmentPropertyAttributeProblems.Add(
                        $"Trying to encaust type {encaustable}: property {p.Name} isn't marked as either Encaust or DontEncaust");

                //is there a matching property of the same name on the command that we're encausting to?
                if (p.IsDefined(typeof(Encaust)) && specifiedGenericCommandType.GetProperty(p.Name) == null)
                    encaustmentPropertyAttributeProblems.Add(
                        $"Encaustable of type {CachedEncaustableType} has Encaust-marked property {p.Name}, but the command we're encausting to ({specifiedGenericCommandType}) doesn't have a property with a matching name.");
            }

            foreach (var cp in specifiedGenericCommandType.GetProperties())
            {
                var candidateEncaustableProperty = propertiesToEncaust.SingleOrDefault(p => p.Name == cp.Name);
                if (candidateEncaustableProperty == null)
                    encaustmentPropertyAttributeProblems.Add(
                       $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {CachedEncaustableType} lacks, so we can't encaust the other to the one.");
                else if (candidateEncaustableProperty.IsDefined(typeof(DontEncaust)))
                    encaustmentPropertyAttributeProblems.Add(
                        $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {CachedEncaustableType} marks as DontEncaust, so we can't encaust the other to the one.");
            }

            return encaustmentPropertyAttributeProblems;
        }

        private IsEmulousEncaustable GetEmulousEncaustableAttributeFromInstance(IEncaustable encaustable)
        {
            IsEmulousEncaustable emulousEncaustableAttribute;
            emulousEncaustableAttribute =
                encaustable.GetType().GetCustomAttributes(typeof(IsEmulousEncaustable), false).SingleOrDefault() as
                    IsEmulousEncaustable;
            return emulousEncaustableAttribute;
        }

        private IsEncaustableClass GetEncaustableClassAttributeFromEncaustableInstance(IEncaustable encaustable)
        {
            IsEncaustableClass isEncaustableClassAttribute;
            isEncaustableClassAttribute =
                encaustable.GetType().GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as
                    IsEncaustableClass;
            return isEncaustableClassAttribute;
        }

        private IsEncaustableClass GetEncaustableClassAttributeFromType(Type encaustableType)
        {
            IsEncaustableClass isEncaustableClassAttribute;
            isEncaustableClassAttribute =encaustableType.GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as
                    IsEncaustableClass;
            return isEncaustableClassAttribute;
        }

        private List<PropertyInfo> GetEncaustableProperties(IEncaustable encaustable)
        {
            var allPropertiesOnEncaustable = GetPropertiesToEncaust(CachedEncaustableType);
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

            foreach (var encaustableProperty in GetEncaustableProperties(encaustable))
            {
                var commandPropertyToSet = typeof(T).GetProperty(encaustableProperty.Name);

                if(isGenericListOfEncaustables(encaustableProperty.PropertyType))
                    commandPropertyToSet.SetValue(command, EncaustListToEncaustedList(encaustable,encaustableProperty));


                //is the property we're trying to encaust itself an encaustable entity in its own right? if so, do it as an inner command
                else if (typeof(IEncaustable).IsAssignableFrom(encaustableProperty.PropertyType))
                {
                    IEncaustable innerEncaustableInstance = encaustableProperty.GetValue(encaustable) as IEncaustable;
                    object innerEncaustedCommand = encaustPropertyAsCommandInItsOwnRight(innerEncaustableInstance);
                        commandPropertyToSet.SetValue(command, innerEncaustedCommand);
                }
                

                else
                    commandPropertyToSet.SetValue(command, encaustableProperty.GetValue(encaustable));



            }

            return command;
        }

        private bool isGenericListOfEncaustables(Type potentialListOfEncaustablesType)
        {
            if (!potentialListOfEncaustablesType.IsGenericType)
                return false;
            if(potentialListOfEncaustablesType.GetGenericTypeDefinition()==typeof(List<>))
                return true;
            return false;
        }
        private object EncaustListToEncaustedList(IEncaustable outerEncaustable, PropertyInfo encaustableListProperty)
        {
            //get a list of all the members, as objects
            IList untypedIntermediateSourceList = encaustableListProperty.GetValue(outerEncaustable, null) as IList;

            //get the type of the encaustable members
            var encaustableTypeArgument = encaustableListProperty.PropertyType.GenericTypeArguments[0];

            //get the type we'll encaust them to, via the attribute on the member type

            var encaustableClassAttribute = GetEncaustableClassAttributeFromType(encaustableTypeArgument);
            Type encaustToType = encaustableClassAttribute.ToType;

            //create a generic list where the encaust-to type is the type argument
            Type destinationListConstructedType = typeof(List<>).MakeGenericType(encaustToType);

            //now instantiate the destination list
            var destinationList = (IList) Activator.CreateInstance(destinationListConstructedType);

            //now encaust each of the members of the intermediate source list, and assign them to the destination list

            foreach (IEncaustable member in untypedIntermediateSourceList)
            {
                object encaustedMember = encaustPropertyAsCommandInItsOwnRight(member);
                destinationList.Add(encaustedMember);
            }

            return destinationList;


        }


        private object encaustPropertyAsCommandInItsOwnRight(IEncaustable innerEncaustableInstance)
        {
            Type encaustInnerAsType = GetEncaustableClassAttributeFromEncaustableInstance(innerEncaustableInstance).ToType; //for an encausted list, this is called each time through - so there's room for caching
            Type baseEncausteryType = typeof(Encaustery<>);
            Type combinedTypeWithGenericArgument = baseEncausteryType.MakeGenericType(encaustInnerAsType);

            object innerEncaustery = Activator.CreateInstance(combinedTypeWithGenericArgument);
            var encaustMethod = innerEncaustery.GetType().GetMethod("Encaust");
            var encaustedInnerCommand = encaustMethod.Invoke(innerEncaustery, new object[] { innerEncaustableInstance });

            return encaustedInnerCommand;

        }





        /// <summary>
        /// If we pass eg something inherited from Monobehaviour for encausting, it'll bring a lot of properties we don't want to encaust
        /// </summary>
        /// <param name="encaustable"></param>
        /// <returns></returns>
        private static PropertyInfo[] GetPropertiesToEncaust(Type encaustableType)
        {
  
            //BindingFlags.DeclaredOnly means you also need to be specific about which kinds of methods you want to return
            var encaustableProperties = encaustableType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            return encaustableProperties;
        }
    }
}
