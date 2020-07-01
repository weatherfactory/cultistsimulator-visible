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
 

        public abstract bool TryImportProperty<T>(T entity, CachedFucineProperty<T> property, Hashtable entityData, ContentImportLog log) where T:AbstractEntity<T>;

    }
}
