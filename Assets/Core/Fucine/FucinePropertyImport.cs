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
        protected ContentImportLogger _logger;
        protected PropertyInfo _property;

        protected FucineImport(PropertyInfo property, ContentImportLogger logger)
        {
            _property = property;
            _logger = logger;
        }

        public abstract void Populate(IEntity entity, Hashtable entityData,
            Type entityType);

        public static FucineImport CreateInstance(PropertyInfo property,ContentImportLogger logger,Hashtable entityData)
        {
                        if (entityData.ContainsKey(property.Name))
                        {
                            if (Attribute.GetCustomAttribute(property, typeof(FucineId)) is FucineId)
                                return new FucineImportId(property,logger);


                            else if (Attribute.GetCustomAttribute(property, typeof(FucineList)) is FucineList)
                                return new FucineImportList(property, logger);
            
                            else if (Attribute.GetCustomAttribute(property, typeof(FucineDict)) is FucineDict)
                                return new FucineImportDict(property, logger);

                            else if (Attribute.GetCustomAttribute(property, typeof(FucineAspects)) is FucineAspects)
                                return new FucineImportAspects(property, logger);

                            else if (Attribute.GetCustomAttribute(property, typeof(FucineSubEntity)) is FucineSubEntity)
                                return new FucineImportSubEntity(property, logger);

                            else if(Attribute.GetCustomAttribute(property, typeof(FucineValue)) is FucineValue)
                                return new FucineImportValue(property, logger);
                            else
                                throw new ApplicationException("Unknown Fucine property type on: " + property.Name);
                

                        }
                        else
                        {
                            return new FucineImportDefault(property, logger);
                        }
        }
    }
}
