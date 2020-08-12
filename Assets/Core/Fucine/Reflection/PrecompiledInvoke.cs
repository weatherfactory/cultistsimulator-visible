using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Assets.Core.Fucine.DataImport;

namespace Assets.Core.Fucine
{
    public static class PrecompiledInvoke
    {

        public static Func<TEntity,object> BuildGetter<TEntity>(PropertyInfo propertyInfo) where TEntity : AbstractEntity<TEntity>
        {
            var targetType = propertyInfo.DeclaringType; //this is the type of the class object of which the property is a member
            if (targetType == null)
                throw new ApplicationException("Import error: can't find a declaring type for property " + propertyInfo.Name);

            var exInstance = Expression.Parameter(targetType, "t"); //t.PropertyName
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, propertyInfo);

            //t.propertyValue(Convert(p))
            var exConvertToObject = Expression.Convert(exMemberAccess, typeof(object));
            var lambda = Expression.Lambda<Func<TEntity,object>>(exConvertToObject, exInstance);
            var func = lambda.Compile();
            return func;
        }

        public static Action<TEntity, object> BuildSetter<TEntity>(PropertyInfo propertyInfo) where TEntity:AbstractEntity<TEntity>
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

        public static Func<T> BuildDefaultConstructor<T>()
        {
            //var exValue = Expression.Parameter(typeof(TEntity), "t");
            var constructorInfo = typeof(T).GetConstructor(Type.EmptyTypes);

            if (constructorInfo == null)
                return null;

            var exBody = Expression.New(constructorInfo);

            var lambda = Expression.Lambda<Func<T>>(exBody);
            var func = lambda.Compile();
            return func;
        }

        public static Func<EntityData,ContentImportLog, TEntity> BuildEntityConstructor<TEntity>() where TEntity : AbstractEntity<TEntity>
        {
            //var exValue = Expression.Parameter(typeof(TEntity), "t");
            Type[] constructorTypeArgs = {typeof(EntityData), typeof(ContentImportLog)};

            var constructorInfo = typeof(TEntity).GetConstructor(constructorTypeArgs);

            if (constructorInfo == null)
                throw new ApplicationException($"Couldn't find a suitable entity constructor for {typeof(TEntity).Name}. This can probably be fixed by adding a  public {typeof(TEntity).Name}(EntityData importDataForEntity, ContentImportLog log):base(importDataForEntity, log) constructor. There is a cleaner way to do this so I don't need an explicit constructor - TODO :)");

            var exDataParam = Expression.Parameter(typeof(EntityData), "entitydata");
            var exLogParam = Expression.Parameter(typeof(ContentImportLog), "log");


            var exBody = Expression.New(constructorInfo, exDataParam, exLogParam);
            


            var lambda = Expression.Lambda<Func<EntityData, ContentImportLog, TEntity>>(exBody, exDataParam, exLogParam);
            var func = lambda.Compile();
            return func;
        }


    }
}