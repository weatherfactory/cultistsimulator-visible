using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class NullElement: Element
    {
        public const string NULL_ELEMENT_ID = "NULL_ELEMENT_ID";

        public NullElement()
        {
            _id = NULL_ELEMENT_ID;
        }

        public static NullElement Create()
        {
            return new NullElement();
        }
    }
}
