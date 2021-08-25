using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.Assets.Scripts.Application.UI.Otherworlds.Ceremonies
{
    public class EndingContentCeremony: AbstractOtherworldCeremony
    {
        [SerializeField] private TextMeshProUGUI Title;
        [SerializeField] private TextMeshProUGUI Description;
        [SerializeField] private Image Image;
    }
}
