using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Assets.Core.Fucine
{
    public class CachedFucineProperty
    {

        public PropertyInfo PropertyInfo { get; set; }
        public Fucine FucineAttribute { get; set; }
        public string Name => PropertyInfo.Name.ToLowerInvariant();

        public CachedFucineProperty()
        {
        
        }
    }


    public static class FastInvoke
    {
        public static Func<T, object> BuildUntypedSetter<T>(PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.DeclaringType; //this is the class object, not the instance, so I think we can reuse it OK

            var exInstance = Expression.Parameter(targetType, "t"); //t.PropertyName
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, propertyInfo);
            //t.propertyValue(Convert(p))
            var exValue = Expression.Parameter(typeof(object), "p");
            var exConvertedValue=Expression.Convert(exValue,propertyInfo.PropertyType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
            var lambda=Expression.Lambda<Action<T,object>> (exBody, exInstance, exValue);
            var action = lambda.Compile();
            return action;
        }

}
}