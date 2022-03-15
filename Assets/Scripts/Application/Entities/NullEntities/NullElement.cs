using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class NullElement: Element
    {
        public const string NULL_ELEMENT_ID = "NULL_ELEMENT_ID";
        private static NullElement _instance;
        protected NullElement()
        {
            _id = NULL_ELEMENT_ID;
            ManifestationType = "Null";
        }

        public override bool IsValid()
        {
            return false;
        }

        public static NullElement Create()
        {
            if(_instance==null)
                _instance=new NullElement();
            return _instance;
        }
    }
}
