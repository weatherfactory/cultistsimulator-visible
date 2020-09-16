using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;
using TMPro;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class SecretHistoryLogMessageEntry:MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI TextComponent;


        public void DisplayMessage(NoonLogMessage message)
        {
            TextComponent.text = message.Description;

            if (message.MessageLevel == 0)
                TextComponent.color = AspectColor.Knock();

            if (message.MessageLevel == 1)
                TextComponent.color = AspectColor.Lantern();

            else if (message.MessageLevel == 2)
                TextComponent.color = AspectColor.Forge();
        }
    }
}
