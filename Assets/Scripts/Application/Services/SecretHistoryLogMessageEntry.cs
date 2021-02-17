using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using TMPro;
using UnityEngine;

namespace SecretHistories.Services
{
    public class SecretHistoryLogMessageEntry:MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI TextComponent;
#pragma warning restore 649

        private ILogMessage _message;
        public int Count { get; private set; }

        public bool TryMatchMessage(ILogMessage messageToCheck)
        {
            if (_message.Description == messageToCheck.Description)
            {
                Count++;
                DisplayMessage(messageToCheck);
                return true;
            }

            return false;
        }

        public void DisplayMessage(ILogMessage message)
        {
            
            _message = message;

            if (Count > 0)
                TextComponent.text = $"({Count}) {message.Description}";
            else
                TextComponent.text = message.Description;

            if (message.MessageLevel == 0)
                TextComponent.color = AspectColor.Knock();

            if (message.MessageLevel == 1)
                TextComponent.color = AspectColor.Lantern();

            else if (message.MessageLevel == 2)
                TextComponent.color = AspectColor.Forge();


            Count++;

        }
    }
}
