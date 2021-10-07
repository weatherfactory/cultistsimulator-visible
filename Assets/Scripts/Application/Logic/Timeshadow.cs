using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;

namespace SecretHistories.Logic
{
    public class Timeshadow
    {
        private float _lifetimeRemaining;
        public float Lifetime { get; }

        public float LifetimeRemaining => Math.Max(0,_lifetimeRemaining);
        public float LifetimeSpent => Math.Max(0, Lifetime - LifetimeRemaining);

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
            _lifetimeRemaining = lifetimeRemaining;
            Resaturate = resaturate;
        }

        public void SpendAllRemainingTime()
        {
            SpendTime(_lifetimeRemaining);
        }

        public void SpendTime(float interval)
        {
            LastInterval = interval;
            if (Transient)
            {
                _lifetimeRemaining -= interval;
            }
        }
    }
}
