using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Constants.Events
{

    public class ManifestationInteractionEventArgs
    {
        public Sphere Sphere { get; set; }
        public Element Element { get; set; }
        public IManifestation Manifestation { get; set; }
        public PointerEventData PointerEventData { get; set; }
        public Context Context { get; set; }
        public Interaction Interaction { get; set; }

        public ManifestationInteractionEventArgs()
        {
            Context = new Context(Context.ActionSource.Unknown);
        }

        public ManifestationInteractionEventArgs(Context context)
        {
            Context = context;
        }
    }

}
