using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace SecretHistories.UI
{
    //public interface IRegisterable
    //{
        
    //}

    public class Registry
    {

        private static readonly Dictionary<Type, System.Object> registered=new Dictionary<Type, object>();

        public static T Get<T>(bool logWarningIfNotRegistered=true) where T: class
        {

            if (registered.ContainsKey(typeof(T)))
            {

                T matchingTypeInstance = registered[typeof(T)] as T;

                return matchingTypeInstance;
            }

            if (typeof(T).IsInterface)
            {
                foreach (var candidateType in registered.Keys)
                    if (typeof(T).IsAssignableFrom(candidateType))
                    {
                        T matchingSubtypeInstance = registered[candidateType] as T;
                        return matchingSubtypeInstance;
                    }

            }


            if (typeof(T).IsAbstract)
            {
                foreach(var candidateType in registered.Keys)
                    if(candidateType.IsSubclassOf(typeof(T)))
                    {
                        T matchingSubtypeInstance = registered[candidateType] as T;
                        return matchingSubtypeInstance;
                    }
                    
            }

            //fallbacks
            if (typeof(T)==typeof(LanguageManager))
                return new NullLocStringProvider() as T;

            if (typeof(T) == typeof(SphereCatalogue))
            {
                var tcc=new SphereCatalogue();
                registered.Add(typeof(T),tcc);
                return tcc as T;
            }

            if (logWarningIfNotRegistered)
                NoonUtility.Log(typeof(T).Name + " wasn't registered: returning null",2);
                
            return null;

        }



        public void Register<T>(T toRegister) where T: class
        {
            registered[typeof(T)] = toRegister;
        }


    }
}
