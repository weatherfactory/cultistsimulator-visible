using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Assets.Core.Fucine
{
    public class CachedFucineProperty<T>
    {

        public PropertyInfo PropertyInfo { get; }
        public Fucine FucineAttribute { get; }
        public string LowerCaseName { get; }
        public Action<T, object> FastInvokeSetter { get; } //should this be abstractsetter or propertyinfo?


        public CachedFucineProperty(PropertyInfo propertyInfo, Fucine fucineAttribute)
        {
            PropertyInfo = propertyInfo;
            FucineAttribute = fucineAttribute;
            LowerCaseName = propertyInfo.Name.ToLowerInvariant();
           if(propertyInfo.CanWrite)

               FastInvokeSetter = FastInvoke.BuildUntypedSetter<T>(propertyInfo);
        }

        public AbstractImporter GetImporterForProperty(ContentImportLog log)
        {
            return null;
        }
    }


    public static class FastInvoke
    {
        public static Action<T, object> BuildUntypedSetter<T>(PropertyInfo propertyInfo) 
        {
            var targetType = propertyInfo.DeclaringType; //this is the type of the class object of which the property is a member
            if(targetType==null)
                throw new ApplicationException("Import error: can't find a declaring type for property " + propertyInfo.Name);

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