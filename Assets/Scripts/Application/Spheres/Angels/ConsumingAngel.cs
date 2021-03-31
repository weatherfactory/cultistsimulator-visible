using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.UI;

namespace SecretHistories.Spheres.Angels
{
    public class ConsumingAngel : IAngel
    {
        private ThresholdSphere _watchingOverThreshold;

        public int Authority => 9;

        public void Act(float interval)
        {
        if (_watchingOverThreshold == null)
                return;
        }

        public void SetWatch(Sphere sphere)
        {
            _watchingOverThreshold = sphere as ThresholdSphere;
            if(_watchingOverThreshold == null)
                NoonUtility.LogWarning($"tried to set a consuming angel to watch over sphere {sphere.Id}, but it isn't a threshold sphere, so that won't work.");
            else
            {if(_watchingOverThreshold.ConsumingIcon!=null)
                    _watchingOverThreshold.ConsumingIcon.SetActive(true);
            }
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            if (context.actionSource == Context.ActionSource.FlushingTokens)
            {
                token.Retire(RetirementVFX.CardBurn);
                return true;
            }

            return false;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            return false;
        }

        public void Retire()
        {
            Defunct = true;
        }

        public bool Defunct { get; protected set; }
    }
}
