using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Fucine;

namespace SecretHistories.NullObjects
{
    public class NullVerb:Verb
    {

        private static NullVerb _instance;
        
        public override bool IsValid()
        {
            return false;
        }


        public static NullVerb Create()
        {
            if(_instance==null)
                _instance= new NullVerb();

            return _instance;
        }

        protected NullVerb()
        {
            Thresholds=new List<SphereSpec>();
            SetId(string.Empty);
            Label = string.Empty;
            Description = string.Empty;
        }







    }
}
