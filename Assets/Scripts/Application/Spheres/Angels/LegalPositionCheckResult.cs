using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Choreographers
{
   public class LegalPositionCheckResult
    {
        public bool IsLegal = false;
        public string BlockerName;
        public Rect BlockerRect;


        public static LegalPositionCheckResult Legal()
        {
            return new LegalPositionCheckResult() { IsLegal = true };
        }

        public static LegalPositionCheckResult Illegal()
        {
            return new LegalPositionCheckResult();
        }

        public static LegalPositionCheckResult Blocked(string name, Rect rect)
        {
            return new LegalPositionCheckResult() { BlockerName = name, BlockerRect = rect };
        }
    }
}
