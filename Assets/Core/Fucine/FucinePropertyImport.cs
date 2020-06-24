using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   
    public abstract class FucineImport
    {
        protected ContentImportLog Log;
        protected PropertyInfo _property;

        protected FucineImport(PropertyInfo property, ContentImportLog log)
        {
            _property = property;
            Log = log;
        }

        public abstract void Populate(AbstractEntity entity, Hashtable entityData,
            Type entityType);

        public static FucineImport CreateInstance(PropertyInfo property,ContentImportLog log,Hashtable entityData)
        {
            if (Attribute.GetCustomAttribute(property, typeof(FucineId)) is FucineId)
                return new FucineImportId(property, log);
            //Try whether the key exists or not: we might be using an arbitrary internal key

            else if (entityData.ContainsKey(property.Name))
            {
                if (Attribute.GetCustomAttribute(property, typeof(FucineList)) is FucineList)
                    return new FucineImportList(property, log);
            
                else if (Attribute.GetCustomAttribute(property, typeof(FucineDict)) is FucineDict)
                    return new FucineImportDict(property, log);

                else if (Attribute.GetCustomAttribute(property, typeof(FucineAspects)) is FucineAspects)
                    return new FucineImportAspects(property, log);

                else if (Attribute.GetCustomAttribute(property, typeof(FucineSubEntity)) is FucineSubEntity)
                    return new FucineImportSubEntity(property, log);

                else if(Attribute.GetCustomAttribute(property, typeof(FucineValue)) is FucineValue)
                    return new FucineImportValue(property, log);
                else
                    throw new ApplicationException("Unknown Fucine property type on: " + property.Name);
                

            }
            else
            {
                return new FucineImportDefault(property, log);
            }
        }
    }
}
