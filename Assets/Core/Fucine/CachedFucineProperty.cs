using System;
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


        public void SetViaFastInvoke(TTarget target, IAspectsDictionary value)
        {
                FastInvokeSetter(target, value);
        }

        public void SetViaFastInvoke(TTarget target,object value)
        {
      if(ThisPropInfo.PropertyType.IsEnum)
          FastInvokeSetter(target,Enum.ToObject(ThisPropInfo.PropertyType,value));
      else
            FastInvokeSetter(target, Convert.ChangeType(value,ThisPropInfo.PropertyType));
        }

    }
}