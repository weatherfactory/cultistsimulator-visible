using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{
    public class NullPortal: Portal
    {
        public const string NULL_PORTAL_ID = "NULL_PORTAL_ID";

        public override string Icon => ".";

        private static NullPortal _instance;
        protected NullPortal()
        {
            _id = NULL_PORTAL_ID;
        }

        public static NullPortal Create()
        {
            if (_instance == null)
                _instance = new NullPortal();
            return _instance;
        }
    }
}
