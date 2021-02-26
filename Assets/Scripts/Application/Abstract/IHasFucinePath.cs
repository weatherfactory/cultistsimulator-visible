using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace SecretHistories.Abstract
{
    public interface IHasFucinePath
    {
        string Id { get; }
        FucinePath GetAbsolutePath();
    }
}
