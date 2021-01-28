using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;

namespace Assets.Scripts.Application.Logic
{
    public class Timeshadow
    {
        public float Lifetime { get; }
        public float LifetimeRemaining{ get; private set; }
        public float LastInterval { get; private set; }
        public bool Transient => Lifetime > 0;
        public bool Resaturate { get; }
        public EndingFlavour EndingFlavour { get; set; }

        public static Timeshadow CreateTimelessShadow()
        {
            var ts = new Timeshadow(0, 0, false);
            return ts;
        }
        public Timeshadow(float lifetime, float lifetimeRemaining, bool resaturate)
        {
            Lifetime = lifetime;
            LifetimeRemaining = lifetimeRemaining;
            Resaturate = resaturate;
        }

        public void SpendTime(float interval)
        {
            LastInterval = interval;
            LifetimeRemaining -= interval;
        }
    }
}
