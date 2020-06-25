using System;
using System.Collections.Generic;

namespace Assets.Core.Fucine
{
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