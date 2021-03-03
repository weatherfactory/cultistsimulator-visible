using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Manifestations;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class NullElement: Element
    {
        public const string NULL_ELEMENT_ID = "NULL_ELEMENT_ID";
        public static NullElement _instance;
        public NullElement()
        {
            _id = NULL_ELEMENT_ID;
            ManifestationType = "Null";
        }

        public static NullElement Create()
        {
            if(_instance==null)
                _instance=new NullElement();
            return _instance;
            }
    }
}
