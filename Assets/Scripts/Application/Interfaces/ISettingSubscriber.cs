using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Fucine
{
    public interface ISettingSubscriber
    {
        void WhenSettingUpdated(object newValue);
    }
}
