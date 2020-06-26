using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public class CachedFucineProperty<TTarget> where TTarget : AbstractEntity<TTarget>
    {

        public PropertyInfo ThisPropInfo { get; }
        public Fucine FucineAttribute { get; }
        public string LowerCaseName { get; }
        private readonly Action<TTarget, object> FastInvokeSetter;


        public CachedFucineProperty(PropertyInfo thisPropInfo, Fucine fucineAttribute)
        {
            ThisPropInfo = thisPropInfo;
            FucineAttribute = fucineAttribute;
            LowerCaseName = thisPropInfo.Name.ToLowerInvariant();
           if(thisPropInfo.CanWrite)

               FastInvokeSetter = FastInvoke.BuildUntypedSetter<TTarget>(thisPropInfo);
        }

        public AbstractImporter GetImporterForProperty()
        {
            return FucineAttribute.CreateImporterInstance();
        }


        public void SetValueFastInvoke(TTarget target,object value)
        {
      if(ThisPropInfo.PropertyType.IsEnum)
          FastInvokeSetter(target,Enum.ToObject(ThisPropInfo.PropertyType,value));
      else
            FastInvokeSetter(target, Convert.ChangeType(value,ThisPropInfo.PropertyType));
        }

    }


    public static class FastInvoke
    {
        public static Action<TEntity, object> BuildUntypedSetter<TEntity>(PropertyInfo propertyInfo) where TEntity:AbstractEntity<TEntity>
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
            var lambda=Expression.Lambda<Action<TEntity,object>> (exBody, exInstance, exValue);
            var action = lambda.Compile();
            return action;
        }


    }




}