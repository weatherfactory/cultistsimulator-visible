﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Fucine;

namespace SecretHistories.NullObjects
{
    public class NullVerb:IVerb
    {
        public string Id { get; private set; }
        public void SetId(string id)
        {
            Id = id;
        }

        public string Label { get; set; }
        public string Description { get; set; }
        
        public bool Transient { get; }
        public string Art => string.Empty;


        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(NullManifestation);
        }

        public List<SphereSpec> Thresholds { get; set; }

        public bool Startable { get; }
        public bool ExclusiveOpen => false;

        
        public Situation CreateDefaultSituation(TokenLocation anchorLocation)
        {
            return new NullSituation(SituationPath.NullPath());
        }

        public bool AllowMultipleInstances => true;

        protected NullVerb()
        {
            Thresholds=new List<SphereSpec>();
            Startable = false;
        }


        public static NullVerb Create()
        {
            return new NullVerb();
        }
    }
}
