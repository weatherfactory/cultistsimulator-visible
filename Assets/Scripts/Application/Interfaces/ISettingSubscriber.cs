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

        //for arrays, we need to know the previous value to correctly remove it; hopefully will find other uses too! 
        //(has default implementation to avoid editing every single ISettingSubscriber in the game)
        void BeforeSettingUpdated(object oldValue) { }
    }
}
