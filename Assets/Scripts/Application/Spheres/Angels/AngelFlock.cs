using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;

namespace SecretHistories.Spheres.Angels

{
    public class AngelFlock
    {
        protected readonly HashSet<IAngel> _angels = new HashSet<IAngel>();
        public void AddAngel(IAngel angel)
        {
            _angels.Add(angel);
        }

        public void RemoveAngel(IAngel angel)
        {
            angel.Retire();
            _angels.Remove(angel);
        }

        public void Act(float seconds, float metaseconds)
        {
            foreach (var angel in _angels)
            {
                angel.Act(seconds, metaseconds);
            }
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            foreach (var a in _angels.OrderByDescending(angel => angel.Authority))
                if (a.MinisterToDepartingToken(token, context))
                    return true;

            return false;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            foreach (var a in _angels.OrderByDescending(angel=>angel.Authority))
                if (a.MinisterToEvictedToken(token, context))
                    return true;

            return false;
        }
    }
}
