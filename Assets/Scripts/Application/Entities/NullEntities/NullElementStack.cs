using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.UI;


namespace SecretHistories.Elements
{
    public class NullElementStack: ElementStack
    {

        public override bool CanMergeWith(ElementStack intoStack)
        {
            return false;
        }

        public NullElementStack()
        {
            Element=new NullElement();
        }


    }
}
