using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Interfaces
{
    public interface IQuickSpecEntity
    {
        /// <summary>
        /// Populate a default instance based on a single value
        /// </summary>
        /// <param name="value"></param>
        void QuickSpec(string value);
    }
}
