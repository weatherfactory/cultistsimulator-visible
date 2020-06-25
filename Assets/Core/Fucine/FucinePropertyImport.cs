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
        protected CachedFucineProperty _cachedFucinePropertyToPopulate;

        protected FucineImport(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log)
        {
            _cachedFucinePropertyToPopulate = cachedFucinePropertyToPopulate;
            Log = log;
        }

        public abstract void Populate(AbstractEntity entity, Hashtable entityData,
            Type entityType);

        public static FucineImport CreateInstance(CachedFucineProperty property,ContentImportLog log,Hashtable entityData)
        {
            if (property.FucineAttribute is FucineId)
                return new FucineImportId(property, log);
            //Try whether the key exists or not: we might be using an arbitrary internal key

            else if (entityData.ContainsKey(property.Name))
            {
                if (property.FucineAttribute is FucineList)
                    return new FucineImportList(property, log);
            
                else if (property.FucineAttribute is FucineDict)
                    return new FucineImportDict(property, log);

                else if (property.FucineAttribute is FucineAspects)
                    return new FucineImportAspects(property, log);

                else if (property.FucineAttribute is FucineSubEntity)
                    return new FucineImportSubEntity(property, log);

                else if(property.FucineAttribute is FucineValue)
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
