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
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Fucine;

namespace SecretHistories.NullObjects
{
    public class NullVerb:Verb
    {
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        public int Quantity =>0;
        public Dictionary<string, int> Mutations { get; }


        
        public string Icon => string.Empty;

        
        public List<SphereSpec> Thresholds { get; set; }

        public override bool IsValid()
        {
            return false;
        }
        public NullVerb()
        {
            Thresholds=new List<SphereSpec>();
            SetId(string.Empty);
            Label = string.Empty;
            Description = string.Empty;
        }


        public static NullVerb Create()
        {
            return new NullVerb();
        }





    }
}
