using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecretHistories.Interfaces
{
    public interface ISaveable
    {
        Hashtable GetSaveData();
    }
}
