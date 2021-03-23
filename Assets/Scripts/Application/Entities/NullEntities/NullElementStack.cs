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
        private static NullElementStack _instance;


        public override bool CanMergeWith(ITokenPayload otherPayload)
        {
            return false;
        }

        public override bool IsValid()
        {
            return false;
        }

        public override bool IsValidElementStack()
        {
            return false;

        }

        protected NullElementStack()
        {
            Element = NullElement.Create();
        }

        public static NullElementStack Create()
        {
            if(_instance==null)
                _instance=new NullElementStack();
            return _instance;
        }
    }
}
