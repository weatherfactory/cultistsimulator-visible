using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure.Modding
{
    public class ModsDisplayPanel: MonoBehaviour
    {
        //This finally made it into a class of its own. A certain amount of refactoring would naturally move other logic in here.

        [SerializeField]
        public TextMeshProUGUI restartAfterModChangeWarning;
    }
}
