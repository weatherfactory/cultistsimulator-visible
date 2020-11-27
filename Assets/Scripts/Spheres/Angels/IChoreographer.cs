using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.Scripts.Spheres.Angels
{
    public interface IChoreographer
    {



        void PlaceTokenAtFreePosition(Token token, Context context);

        /// <summary>
        /// Place at a specific position, pushing other tokens out of the way if necessary
        /// </summary>
        void PlaceTokenAssertivelyAtSpecifiedPosition(Token token, Context context, Vector2 pos);

        /// <summary>
        /// Place as close to a specific position as we can get
        /// </summary>
        void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos);

        Vector2 GetFreePosWithDebug(Token token, Vector2 centerPos, int startIteration = -1);

    }
}
