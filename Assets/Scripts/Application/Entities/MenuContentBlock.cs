using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Entities
{
    public class MenuContentBlock: MonoBehaviour
    {
        public int ID;
        public string URL;

        public void ShowPromo()
        {
            SoundManager.PlaySfx("UIButtonClick");
            UnityEngine.Application.OpenURL(URL);
        }
    }


}
