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
        /// Everything that exists in Fucine space also exists in the Unity hierarchy
        /// </summary>
        /// <returns></returns>
        RectTransform GetReferenceRectTransform();
    }
}
