using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.UI.Situation
{
    public class SituationCountdownDisplay: MonoBehaviour, ISituationSubscriber
    {
        [SerializeField] Image countdownBar;
        [SerializeField] TextMeshProUGUI countdownText;

        public void SituationStateChanged(SecretHistories.Entities.Situation situation)
        {
        
        }

        public void TimerValuesChanged(SecretHistories.Entities.Situation s)
        {
            Color barColor =
                    UIStyle.GetColorForCountdownBar(s.Recipe.SignalEndingFlavour, s.TimeRemaining);

                countdownBar.color = barColor;
                countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (s.TimeRemaining / s.Warmup));
                countdownText.color = barColor;
                countdownText.text =
                    Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(s.TimeRemaining);
                countdownText.richText = true;
        }

        public void SituationSphereContentsUpdated(SecretHistories.Entities.Situation s)
        {
  
        }

        public void ReceiveNotification(INotification n)
        {

        }

        public void ReceiveCommand(IAffectsTokenCommand command)
        {
            //can't make use of it
        }
    }
}
