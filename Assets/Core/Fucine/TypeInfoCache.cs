using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;

namespace Assets.Core.Fucine
{
    //Credit Florian Doyon: using a generic in a static class means that a different static instance is created (with the private constructor each time) for each distinct type
    //argument used when the class is referenced
    public static class TypeInfoCache<T> where T: AbstractEntity<T>
    {
        // ReSharper disable once StaticMemberInGenericType - ReSharper is concerned we might not realise that a distinct field is stored for each different type argument
        private static readonly HashSet<CachedFucineProperty<T>> FucinePropertiesForType = new HashSet<CachedFucineProperty<T>>();
        //   private static readonly List<string> FucinePropertyNamesForType= new List<string>();

        public static AbstractEntity<T> CreateEntity(Hashtable importDataForEntity, ContentImportLog log)
        {
            if (typeof(T) == typeof(BasicVerb))
                return new BasicVerb(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(DeckSpec))
                return new DeckSpec(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Element))
                return new Element(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Ending))
                return new Ending(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Expulsion))
                return new Expulsion(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Legacy))
                return new Legacy(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(LinkedRecipeDetails))
                return new LinkedRecipeDetails(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(MorphDetails))
                return new MorphDetails(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(MutationEffect))
                return new MutationEffect(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Recipe))
                return new Recipe(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(SlotSpecification))
                return new SlotSpecification(importDataForEntity, log) as AbstractEntity<T>;
            else
            {
                throw new ApplicationException("Don't know about" + typeof(T).Name);
            }

        }



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
                    CachedFucineProperty<T> cachedProperty = new CachedFucineProperty<T>( thisProperty, fucineAttribute);
                    FucinePropertiesForType.Add(cachedProperty);
                  //  FucinePropertyNamesForType.Add(cachedProperty.LowerCaseName);
                }
            }
        }

            public static HashSet<CachedFucineProperty<T>> GetCachedFucinePropertiesForType()
        {
            //This will return the cached results for the type referenced as a parameter when calling the static class - 
            //so we don't need to specify it again here
            return FucinePropertiesForType;
        }
    }



}