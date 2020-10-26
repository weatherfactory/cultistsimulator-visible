using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Infrastructure.Events
{

    public class ContainerStacksChangedArgs
    {
        public TokenContainer Container { get; set; }
        //room for eg a diff or the nature of the change
    }

    [Serializable]
    public class ContainerStacksChangedEvent : UnityEvent<ContainerStacksChangedArgs>
    {
    }


}
