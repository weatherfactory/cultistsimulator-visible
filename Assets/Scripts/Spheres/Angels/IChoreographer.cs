using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
    public interface IChoreographer
    {


        void PlaceTokenAtFreeLocalPosition(Token token, Context context);

        /// <summary>
        /// Place at a specific position, pushing other tokens out of the way if necessary
        /// </summary>
        void PlaceTokenAssertivelyAtSpecifiedLocalPosition(Token token, Context context, Vector2 pos);

        /// <summary>
        /// Place as close to a specific position as we can get
        /// </summary>
        void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 pos);

        Vector2 GetFreeLocalPosition(Token token, Vector2 centerPos, int startIteration = -1);

    }
}
