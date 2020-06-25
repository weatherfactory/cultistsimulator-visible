using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   
    public abstract class AbstractFucineImporter
    {
        protected ContentImportLog Log;
        protected CachedFucineProperty _cachedFucinePropertyToPopulate;

        protected AbstractFucineImporter(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log)
        {
            _cachedFucinePropertyToPopulate = cachedFucinePropertyToPopulate;
            Log = log;
        }

        public abstract void Populate(AbstractEntity entity, Hashtable entityData,
            Type entityType);

    }
}
