using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public abstract class AbstractEntity
    {
        protected bool Refined = false;
        protected readonly Hashtable UnknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        /// <summary>
        /// This is run for every top-level entity when the compendium has been completely (re)populated. Use for entities that
        /// need additional population based on data from other entities.
        /// It's not explicitly run for subentities - that's up to individual entities.
        /// Overriding implementations should set refined to true, and not run it if it isn't - this isn't yet enforced
        /// </summary>
        /// <param name="log"></param>
        /// <param name="populatedCompendium"></param>
        public virtual void RefineWithCompendium(ContentImportLog log, ICompendium populatedCompendium)
        {
            if (Refined)
                return;
            Hashtable unknownProperties = PopAllUnknownProperties();

            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    log.LogInfo($"Unknown property in import: {k} for {GetType().Name}");
            }

            Refined = true;
        }

    public virtual void PushUnknownProperty(object key, object value)
     {
            UnknownProperties.Add(key, value);
     }

    public virtual Hashtable PopAllUnknownProperties()
     {
        Hashtable propertiesPopped = CollectionsUtil.CreateCaseInsensitiveHashtable(UnknownProperties);
        UnknownProperties.Clear();
        return propertiesPopped;
     }



    public abstract HashSet<CachedFucineProperty> GetFucinePropertiesCached();

    //{
    //    Type thisType = GetType();


    //    return TypeInfoCache<Type>

    //    var entityTypeProperties = thisType.GetProperties();
    //    var entityTypeFucineProperties = new HashSet<CachedFucineProperty>();




    //    foreach (var thisProperty in entityTypeProperties)
    //    {
    //        if (Attribute.GetCustomAttribute(thisProperty, typeof(Fucine)) is Fucine fucineAttribute)
    //        {
    //            CachedFucineProperty cachedProperty= new CachedFucineProperty {PropertyInfo = thisProperty,FucineAttribute = fucineAttribute};
    //            entityTypeFucineProperties.Add(cachedProperty);
    //        }
    //    }

    //    return entityTypeFucineProperties;

    //}


    }

    public class CachedFucineProperty
    {

        public PropertyInfo PropertyInfo { get; set; }
        public Fucine FucineAttribute { get; set; }
        public string Name => PropertyInfo.Name;
    }

//Credit Florian Doyon: using a generic in a static class means that a different static instance is created (with the private constructor each time) for each distinct type
//argument used when the class is referenced
    public static class TypeInfoCache<T>
    {
        // ReSharper disable once StaticMemberInGenericType - ReSharper is concerned we might not realise that a distinct field is stored for each different type argument
        private static readonly HashSet<CachedFucineProperty> FucinePropertiesForType = new HashSet<CachedFucineProperty>();


        //private constructor - it's a static class
        //when TypeInfoCache<T> is referenced, it'll use reflection to pull that information, first time only.
        //downside is we can't have a runtime Type instance without going back to reflection, so each Entity class needs an explicit compile-time reference to its own type.
        static TypeInfoCache()
        {
            var entityTypeProperties = typeof(T).GetProperties();

            foreach (var thisProperty in entityTypeProperties)
            {
                if (Attribute.GetCustomAttribute(thisProperty, typeof(Fucine)) is Fucine fucineAttribute)
                {
                    CachedFucineProperty cachedProperty = new CachedFucineProperty { PropertyInfo = thisProperty, FucineAttribute = fucineAttribute };
                    FucinePropertiesForType.Add(cachedProperty);
                }
            }

        }

        public static HashSet<CachedFucineProperty> GetCachedFucinePropertiesForType()
        {
            //This will return the cached results for the type referenced as a parameter when calling the static class - 
            //so we don't need to specify it again here
            return FucinePropertiesForType;
        }
    }



}
