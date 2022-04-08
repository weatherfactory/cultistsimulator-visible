using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using SecretHistories.Choreographers;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class ChoreoPosition
    {
        public Vector3 Vector3 { get; set; }
        public LegalPositionCheckResult LegalPositionCheckResult { get; set;}


        public static implicit operator Vector3(ChoreoPosition cp) => cp.Vector3;

        public ChoreoPosition(Vector2 vector2)
        {
            Vector3 = vector2;
        }

        public ChoreoPosition(Vector3 vector3)
        {
            Vector3 = vector3;
        }
    }
}
