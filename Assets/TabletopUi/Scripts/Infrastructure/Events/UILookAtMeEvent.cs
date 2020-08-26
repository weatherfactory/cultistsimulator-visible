using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure.Events
{
    
    [Serializable]
    public class UILookAtMeEvent:UnityEvent<Type>
    {
    }
}
