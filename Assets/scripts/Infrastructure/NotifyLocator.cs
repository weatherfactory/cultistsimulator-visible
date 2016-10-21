using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.scripts.Infrastructure
{
    public interface INotifyLocator
    {
        Vector3 GetNotificationPosition();
    }
}
