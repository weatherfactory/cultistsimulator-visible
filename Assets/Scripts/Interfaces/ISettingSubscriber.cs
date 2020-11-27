using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISettingSubscriber
    {
        void WhenSettingUpdated(object newValue);


    }
}
