using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using UnityEngine;

namespace SecretHistories.UI
{
    public class VisibleCharacteristic: MonoBehaviour

    {
        public VisibleCharacteristicId VisibleCharacteristicId => visibleCharacteristicId;

        [SerializeField] private VisibleCharacteristicId visibleCharacteristicId;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    }
}
