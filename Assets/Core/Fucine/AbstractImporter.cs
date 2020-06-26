using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   
    public abstract class AbstractImporter
    {
        protected ContentImportLog Log;


        public abstract bool TryImport<T>(AbstractEntity<T> entity, CachedFucineProperty<T> property, Hashtable entityData,
            Type entityType,ContentImportLog log) where T:AbstractEntity<T>;

    }
}
