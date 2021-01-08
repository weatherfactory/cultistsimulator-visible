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
            _angels.Remove(angel);
        }

        public void Act(float interval)
        {
            foreach (var angel in _angels)
            {
                angel.Act(interval);
            }
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            foreach (var a in _angels)
                if (a.MinisterToEvictedToken(token, context))
                    return true;

            return false;
        }
    }
}
