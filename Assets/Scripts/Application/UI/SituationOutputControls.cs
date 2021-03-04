using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI
{
    public class SituationOutputControls: MonoBehaviour,ISituationSubscriber
    {
        [SerializeField] TextMeshProUGUI dumpButtonText;
        private string buttonClearResultsDefault = "VERB_COLLECT";
        private string buttonClearResultsNone = "VERB_ACCEPT";

        public void SituationStateChanged(Situation situation)
        {
            //
        }

        public void TimerValuesChanged(Situation s)
        {
            //
        }

        public void SituationSphereContentsUpdated(Situation s)
        {
            var outputSphere = s.GetSingleSphereByCategory(SphereCategory.Output);

            if (outputSphere == null)
                return;

            if (outputSphere.GetElementTokens().Any())
                dumpButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsDefault);
            else
                dumpButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsNone);


        }

        public void ReceiveCommand(IAffectsTokenCommand command)
        {
            //
        }
    }
}
