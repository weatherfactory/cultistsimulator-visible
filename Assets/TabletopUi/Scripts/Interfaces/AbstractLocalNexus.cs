using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Interfaces
{

    

   public abstract class LocalNexus : MonoBehaviour
   {
       [SerializeField] public UnityEvent ViewFilesEvent;
       [SerializeField] public UnityEvent ToggleOptionsEvent;
    }
}
