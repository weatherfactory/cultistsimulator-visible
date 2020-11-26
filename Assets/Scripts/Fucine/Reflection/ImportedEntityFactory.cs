using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public interface IImportedEntityFactory
    {
        object ConstructorFastInvoke(EntityData data, ContentImportLog log);

    }
    
    public class ImportedEntityFactory<T>: IImportedEntityFactory where T: AbstractEntity<T>
    {

        private readonly Func<EntityData,ContentImportLog,T> _fastInvokeConstructor;


        public ImportedEntityFactory()
        {

            _fastInvokeConstructor = PrecompiledInvoke.BuildEntityConstructor<T>();
        }

        public object ConstructorFastInvoke(EntityData data, ContentImportLog log)
        {
           return _fastInvokeConstructor(data,log);
        }

    }


}
