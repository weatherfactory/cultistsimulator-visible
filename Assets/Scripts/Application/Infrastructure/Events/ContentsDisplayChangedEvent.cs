using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure.Events
{
    public class ContentsDisplayChangedArgs
    {

        public int Rows { get; set; }
        public float ExtraHeightRequested { get; set; }
        


    }

    /// <summary>
    /// Useful if we need to inform something higher up the tree about contents change without involving intermediary layouts.
    /// </summary>
    [Serializable]
    public class ContentsDisplayChangedEvent : UnityEvent<ContentsDisplayChangedArgs>
    {
    }
}
