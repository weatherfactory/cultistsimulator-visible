using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Abstract
{
    public interface ICharacterHost
    {
        Transform transform { get; }
    }
}
