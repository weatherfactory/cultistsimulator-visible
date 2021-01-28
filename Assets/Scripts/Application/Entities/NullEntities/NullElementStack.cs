using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.UI;


namespace SecretHistories.Elements
{
    [IsEncaustableClass(typeof(NullElementStackCreationCommand))]
    public class NullElementStack: ElementStack
    {

        public override bool CanMergeWith(ITokenPayload otherPayload)
        {
            return false;
        }

        public NullElementStack()
        {
            Element=new NullElement();
        }


    }
}
