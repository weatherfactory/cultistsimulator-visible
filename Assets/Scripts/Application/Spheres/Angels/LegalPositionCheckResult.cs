using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using UnityEngine;

namespace SecretHistories.Choreographers
{
   public class LegalPositionCheckResult
    {
        public bool IsLegal
        {
            get
            {
                if (Legality == PositionLegality.OK)
                    return true;
                return false;
            }
        }

        public string BlockerName;
        public Rect BlockerRect;
        private PositionLegality _legality;

        public PositionLegality Legality
        {
            get => _legality;
            set => _legality = value;
        }

        public static LegalPositionCheckResult Legal()
        {
            return new LegalPositionCheckResult() { Legality = PositionLegality.OK };
        }

        public static LegalPositionCheckResult Illegal()
        {
            return new LegalPositionCheckResult() {Legality = PositionLegality.OutOfBounds};
        }

        public static LegalPositionCheckResult Blocked(string name, Rect rect)
        {
            return new LegalPositionCheckResult() { Legality = PositionLegality.Blocked, BlockerName = name, BlockerRect = rect };
        }
    }
}
