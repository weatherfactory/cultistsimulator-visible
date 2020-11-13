using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Entities
{
    public class NullElement: Element
    {
        public const string NULL_ELEMENT_ID = "NULL_ELEMENT_ID";

        public NullElement()
        {
            _id = NULL_ELEMENT_ID;
        }
    }
}
