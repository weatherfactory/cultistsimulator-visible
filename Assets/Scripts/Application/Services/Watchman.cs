using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace SecretHistories.UI
{

    public class Watchman
    {
        public static void ForgetEverything()
        {
            registered.Clear();
        }

        public static void Forget<T>() where T : class
        {
            if (registered.ContainsKey(typeof(T)))
                registered.Remove(typeof(T));
        }

        private static readonly Dictionary<Type, System.Object> registered=new Dictionary<Type, object>();

        public static bool Exists<T>()
        {
            return (registered.ContainsKey(typeof(T)));
        }

        public static T GetOrInstantiate<T>(Transform atTransform) where T : Component
        {
            var existingInstance = Get<T>();
            if (existingInstance != null)
                return existingInstance; //an instance exists: use that.

            //no instance yet exists: create and return that.
            var prefabFactory = Get<PrefabFactory>(); 

            var newlyInstantiatedInstance=prefabFactory?.CreateLocally<T>(atTransform.root); //assuming we have a prefab factory. If we don't, we're stuffed, return null
            registered.Add(typeof(T),newlyInstantiatedInstance);

            return newlyInstantiatedInstance;
        }

        public static T Get<T>() where T: class
        {

            if (Exists<T>())
            {
                T matchingTypeInstance = registered[typeof(T)] as T;

                return matchingTypeInstance;
            }

            //look for registered objects implementing an interface
            if (typeof(T).IsInterface)
            {
                foreach (var candidateType in registered.Keys)
                    if (typeof(T).IsAssignableFrom(candidateType))
                    {
                        T matchingSubtypeInstance = registered[candidateType] as T;
                        return matchingSubtypeInstance;
                    }

            }

            //look for registered objects inheriting from abstract class
            if (typeof(T).IsAbstract)
            {
                foreach(var candidateType in registered.Keys)
                    if(candidateType.IsSubclassOf(typeof(T)))
                    {
                        T matchingSubtypeInstance = registered[candidateType] as T;
                        return matchingSubtypeInstance;
                    }
                    
            }

            //if the type has an immanence attribute, create an object on the fly, register and return it
            ImmanenceAttribute immanenceAttribute =
                (ImmanenceAttribute)typeof(T).GetCustomAttribute(typeof(ImmanenceAttribute), false);

            if (immanenceAttribute != null)
            {
                var immanentObject = Activator.CreateInstance(immanenceAttribute.FallbackType);
                registered.Add(immanenceAttribute.FallbackType,immanentObject);
                return immanentObject as T;
            }

            //fallback hack for LanguageManager
            if (typeof(T)==typeof(LanguageManager))
                return new NullLocStringProvider() as T;

            if (typeof(T) == typeof(ILocStringProvider))
                return new NullLocStringProvider() as T;

            return null;

        }



        public void Register<T>(T toRegister) where T: class
        {
            registered[typeof(T)] = toRegister;
        }


        public static Type LocateManifestationType(string shortManifestationName, Assembly inAssembly)
        {
            string fullTypeName = $"SecretHistories.Manifestations.{shortManifestationName}Manifestation";

            var type=inAssembly.GetType(fullTypeName);
            return type;
        }

        public static Type LocateManifestationType(string shortManifestationName)
        {
       var inAssembly = typeof(Watchman).Assembly;
      
       return LocateManifestationType(shortManifestationName, inAssembly);
        }

    }
}
