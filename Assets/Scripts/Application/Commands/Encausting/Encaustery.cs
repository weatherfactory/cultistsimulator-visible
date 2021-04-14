using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands.Encausting;
using SecretHistories.Commands;
using SecretHistories.Entities;
using UnityEngine;

namespace SecretHistories.Commands
{
    public class Encaustery<T> where T: class, IEncaustment, new()
    {
        EncausteryTypeCache cache = new EncausteryTypeCache();

        public T Encaust(IEncaustable encaustable)
        {
            var encaustmentPropertyAttributeProblems = getEncaustmentPropertyAttributeProblems(encaustable, typeof(T));
            if (encaustmentPropertyAttributeProblems.Any())
                throw new ApplicationException(string.Join("\n", encaustmentPropertyAttributeProblems));
            
            var encaustedCommand = setCommandPropertiesFromEncaustable(encaustable);

         return encaustedCommand;

        }



        private List<string> getEncaustmentPropertyAttributeProblems(IEncaustable encaustable, Type specifiedGenericCommandType)
        {
            List<string> encaustmentPropertyAttributeProblems = new List<string>();

            var propertiesToEncaust = GetPropertiesToEncaust(cache.GetActualEncaustableType(encaustable.GetType()));
            foreach (var p in propertiesToEncaust)
            {
                //are all properties marked either encaust or dontencaust?
                if (!p.IsDefined(typeof(Encaust), false) && !p.IsDefined(typeof(DontEncaust), false))
                    encaustmentPropertyAttributeProblems.Add(
                        $"Trying to encaust type {encaustable}: property {p.Name} isn't marked as either Encaust or DontEncaust");

                //is there a matching property of the same name on the command that we're encausting to?
                if (p.IsDefined(typeof(Encaust)) && specifiedGenericCommandType.GetProperty(p.Name) == null)
                    encaustmentPropertyAttributeProblems.Add(
                        $"Encaustable of type {cache.GetActualEncaustableType(encaustable.GetType())} has Encaust-marked property {p.Name}, but the command we're encausting to ({specifiedGenericCommandType}) doesn't have a property with a matching name.");
            }

            foreach (var cp in specifiedGenericCommandType.GetProperties())
            {
                var candidateEncaustableProperty = propertiesToEncaust.SingleOrDefault(p => p.Name == cp.Name);
                if (candidateEncaustableProperty == null)
                    encaustmentPropertyAttributeProblems.Add(
                       $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {cache.GetActualEncaustableType(encaustable.GetType())} lacks, so we can't encaust the other to the one.");
                else if (candidateEncaustableProperty.IsDefined(typeof(DontEncaust)))
                    encaustmentPropertyAttributeProblems.Add(
                        $"Command type {specifiedGenericCommandType} has a property ({cp.Name}) that encaustable type {cache.GetActualEncaustableType(encaustable.GetType())} marks as DontEncaust, so we can't encaust the other to the one.");
            }

            return encaustmentPropertyAttributeProblems;
        }



        private List<PropertyInfo> GetEncaustableProperties(IEncaustable encaustable)
        {
            var allPropertiesOnEncaustable = GetPropertiesToEncaust(cache.GetActualEncaustableType(encaustable.GetType()));
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

        private T setCommandPropertiesFromEncaustable(IEncaustable encaustable)
        {
            var command=new T();

            foreach (var encaustableProperty in GetEncaustableProperties(encaustable))
            {
                var commandPropertyToSet = typeof(T).GetProperty(encaustableProperty.Name);

                if (isGenericListOfEncaustables(encaustableProperty.PropertyType))
                {
                    object encaustedList = EncaustListToEncaustedList(encaustable, encaustableProperty);
                    if(encaustedList!=null) //which it might be if the list was empty
                        commandPropertyToSet.SetValue(command, encaustedList);
                }
                

                //is the property we're trying to encaust itself an encaustable entity in its own right? if so, do it as an inner command
                else if (typeof(IEncaustable).IsAssignableFrom(encaustableProperty.PropertyType))
                {
                    IEncaustable innerEncaustableInstance = encaustableProperty.GetValue(encaustable) as IEncaustable;
                    object innerEncaustedCommand = encaustPropertyAsCommandInItsOwnRight(innerEncaustableInstance);
                        commandPropertyToSet.SetValue(command, innerEncaustedCommand);
                }
                

                else
                {
                    //if(!encaustableProperty.PropertyType.IsSerializable)
                    //    NoonUtility.LogWarning($"Property {encaustableProperty.Name} on {encaustable.GetType().Name} isn't marked serializable, so the value probably won't be persisted.");
                    commandPropertyToSet.SetValue(command, encaustableProperty.GetValue(encaustable));
                }


            }

            return command;
        }

        private bool isGenericListOfEncaustables(Type potentialListOfEncaustablesType)
        {
            if (!potentialListOfEncaustablesType.IsGenericType)
                return false;

            var genericArguments = potentialListOfEncaustablesType.GetGenericArguments();
            if (genericArguments.Length != 1) //either it's something like a dictionary with >1 type argument, iwc this isn't relevant, or it doesn't have any type arguments, iwc lawks who knows
                return false;

            if (!typeof(IEncaustable).IsAssignableFrom(genericArguments.First()))
                return false; //it may be a generic list, but the type argument isn't encaustable

            if (potentialListOfEncaustablesType.GetGenericTypeDefinition()==typeof(List<>))
                return true;
            return false;
        }
        private object EncaustListToEncaustedList(IEncaustable outerEncaustable, PropertyInfo encaustableListProperty)
        {
            //get a list of all the members, as objects
            IList untypedIntermediateSourceList = encaustableListProperty.GetValue(outerEncaustable, null) as IList;
            if (untypedIntermediateSourceList.Count == 0) //it's not populated. This means it's not worth encausting, and *also*
            //that if the list is an interface implemented by encaustable types, then we have no way to tell what the type to encaust to would be anyway
            //(Unless we expand encaustability to interfaces)
                return null; 

            //get the type of the encaustable members
            var encaustableTypeArgument = untypedIntermediateSourceList[0].GetType();
            //var encaustableTypeArgument = encaustableListProperty.PropertyType.GenericTypeArguments[0];

            //get the type we'll encaust them to, via the attribute on the member type

            Type encaustToType = cache.GetTargetEncaustmentType(encaustableTypeArgument);

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
            Type encaustInnerAsType = cache.GetTargetEncaustmentType(innerEncaustableInstance.GetType());
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
