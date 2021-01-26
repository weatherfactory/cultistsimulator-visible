using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullEnding: Ending
    {
        public NullEnding(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        protected NullEnding()
        {}

        public override bool IsValid()
        {
            return false;
        }

        public static NullEnding Create()
        {
            return new NullEnding();
        }
    }
}
