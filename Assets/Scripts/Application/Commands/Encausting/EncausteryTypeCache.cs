using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;

namespace SecretHistories.Assets.Scripts.Application.Commands.Encausting
{
   public class EncausteryTypeCache
    {
        private Dictionary<Type, Type> CachedActualEncaustableTypes = new Dictionary<Type, Type>();
        private Dictionary<Type, Type> CachedEncaustmentTypes = new Dictionary<Type, Type>();


        public Type GetActualEncaustableType(Type forType)
        {
            if (!CachedActualEncaustableTypes.ContainsKey(forType))
                DetermineEncaustableAndEncaustmentTypesForCaching(forType);
            return CachedActualEncaustableTypes[forType];
        }

        public Type GetTargetEncaustmentType(Type forType)
        {
            if (!CachedEncaustmentTypes.ContainsKey(forType))
                DetermineEncaustableAndEncaustmentTypesForCaching(forType);
            return CachedEncaustmentTypes[forType];
        }

        public void DetermineEncaustableAndEncaustmentTypesForCaching(Type encaustableTypeToInspect)
        {

            var encaustableAttribute = GetEncaustableClassAttributeFromType(encaustableTypeToInspect);
            if (encaustableAttribute != null)
            {
                //this is a standard encaustable type, ie we're going to be encausting from the properties of the type itself...
                CachedActualEncaustableTypes[encaustableTypeToInspect] = encaustableTypeToInspect;
                //...to the encaustment type specified in the attribute
                CachedEncaustmentTypes[encaustableTypeToInspect] = encaustableAttribute.ToType;
                return;
            }
            var emulousAttribute = GetEmulousEncaustableAttributeFromType(encaustableTypeToInspect);
            if (emulousAttribute != null)
            {
                //this is an emulous encaustable type, ie we're going to be looking at the type it's emulating - its base type, most likely - to get the properties for encausting
                CachedActualEncaustableTypes[encaustableTypeToInspect] = emulousAttribute.EmulateBaseEncaustableType;
                var emulatedClassAttribute = GetEncaustableClassAttributeFromType(emulousAttribute.EmulateBaseEncaustableType);
                CachedEncaustmentTypes[encaustableTypeToInspect] = emulatedClassAttribute.ToType;
                return;
            }

            throw new ApplicationException($"trying to encaust a type ({encaustableTypeToInspect}) which isn't marked with either the IsEncaustableClass or IsEmulousEncaustable attributes");
        }

        private IsEmulousEncaustable GetEmulousEncaustableAttributeFromType(Type type)
        {
            IsEmulousEncaustable emulousEncaustableAttribute;
            emulousEncaustableAttribute =
                type.GetCustomAttributes(typeof(IsEmulousEncaustable), false).SingleOrDefault() as
                    IsEmulousEncaustable;
            return emulousEncaustableAttribute;
        }


        public IsEncaustableClass GetEncaustableClassAttributeFromType(Type encaustableType)
        {
            IsEncaustableClass isEncaustableClassAttribute;
            isEncaustableClassAttribute = encaustableType.GetCustomAttributes(typeof(IsEncaustableClass), false).SingleOrDefault() as
                IsEncaustableClass;
            return isEncaustableClassAttribute;
        }


    }
}
