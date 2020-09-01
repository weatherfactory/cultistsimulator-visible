using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISettingSubscriber
    {
        void UpdateValueFromSetting(float newValue);

    }
}
