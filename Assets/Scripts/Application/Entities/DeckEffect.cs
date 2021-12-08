using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Entities;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Entities
{
    public class DeckEffect
    {
        public DeckSpec DeckSpec { get; }
        public int Draws { get; }

        public DeckEffect(DeckSpec deckSpec, int draws)
        {
            DeckSpec = deckSpec;
            Draws = draws;
        }

        private sealed class DeckSpecDrawsEqualityComparer : IEqualityComparer<DeckEffect>
        {
            public bool Equals(DeckEffect x, DeckEffect y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Equals(x.DeckSpec, y.DeckSpec) && x.Draws == y.Draws;
            }

            public int GetHashCode(DeckEffect obj)
            {
                return HashCode.Combine(obj.DeckSpec, obj.Draws);
            }
        }

        public static IEqualityComparer<DeckEffect> DeckSpecDrawsComparer { get; } = new DeckSpecDrawsEqualityComparer();
    }
}