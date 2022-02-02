using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Abstract
{
    public interface IPayloadWindow
    {
        public Vector3 Position { get; }
        public string Title { get; }

        bool IsVisible { get; }
        void Attach(ElementStack elementStack);
        void Attach(Situation situation);

        /// <summary>
        /// Something's happened that affects our spheres. Anything with an interest in its constituent spheres (like tokens travelling to 'em)
        /// will want to know.  It might be that this should go into the Situation.
        /// </summary>
        void NotifySpheresChanged(Context context);
    }
}
