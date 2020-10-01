using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.CS.TabletopUI
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
            if (!registered.ContainsKey(typeof(LanguageManager)))
                return new NullLocStringProvider() as T;

            if(logWarningIfNotRegistered)
                NoonUtility.Log(typeof(T).Name + " wasn't registered: returning null",2);
                
            return null;

        }



        public void Register<T>(T toRegister) where T: class
        {
            registered[typeof(T)] = toRegister;
        }


    }
}
