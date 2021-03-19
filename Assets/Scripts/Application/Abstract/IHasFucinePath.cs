using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using UnityEngine;

namespace SecretHistories.Abstract
{
    public interface IHasFucinePath
    {
        string Id { get; }
        FucinePath GetAbsolutePath();
        /// <summary>
        /// Everything that exists in Fucine space also has a reference position in the Unity hierarchy. This may not actually be the same
        /// as the game object parent of the component; eg, FucineRoot has no reference position of its own, and uses the default sphere
        /// </summary>
        /// <returns></returns>
        RectTransform GetRectTransform();
    }
}
