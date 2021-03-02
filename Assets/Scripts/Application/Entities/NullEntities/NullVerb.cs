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
using SecretHistories.Elements.Manifestations;
using SecretHistories.Fucine;

namespace SecretHistories.NullObjects
{
    public class NullVerb:Verb
    {
        

        
        public override bool IsValid()
        {
            return false;
        }


        public static NullVerb Create()
        {
            return new NullVerb();
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
