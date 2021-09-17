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
        /// Place as close to a specific position as we can get
        /// </summary>
        void PlaceTokenAsCloseAsPossibleToSpecifiedPosition(Token token, Context context, Vector2 targetPosition);

        Vector2 GetFreeLocalPosition(Token token, Vector2 startPos, int startIteration = -1);

        Vector3 SnapToGrid(Vector3 transformLocalPosition);
    }
}
