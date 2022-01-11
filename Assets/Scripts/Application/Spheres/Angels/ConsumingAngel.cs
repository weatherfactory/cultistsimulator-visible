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

        public void Act(float seconds, float metaseconds)
        {
        if (_watchingOverThreshold == null)
                return;
            //the angel consumes the token when it leaves (see MinisterToDepartingToken below) not on the Act method
        }

        public void SetWatch(Sphere sphere)
        {
            _watchingOverThreshold = sphere as ThresholdSphere;
            if(_watchingOverThreshold == null)
                NoonUtility.LogWarning($"tried to set a consuming angel to watch over sphere {sphere.Id}, but it isn't a threshold sphere, so that won't work.");
            else
            {
                _watchingOverThreshold.ShowAngelPresence(this);
            }
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            if (context.actionSource == Context.ActionSource.FlushingTokens)
            {
                token.ApplyExoticEffect(ExoticEffect.BurnPurge);
                
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
            _watchingOverThreshold.HideAngelPresence(this);
            Defunct = true;
        }
        
        public bool Defunct { get; protected set; }



        public void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            var consumingV =
                visibleCharacteristics.Where(v => v.VisibleCharacteristicId == VisibleCharacteristicId.Consuming);

            foreach(var v in consumingV)
                v.Show();
        }

        public void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            var consumingV =
                visibleCharacteristics.Where(v => v.VisibleCharacteristicId == VisibleCharacteristicId.Consuming);

            foreach (var v in consumingV)
                v.Hide();
        }
    }
}
