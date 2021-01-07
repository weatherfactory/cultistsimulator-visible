using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Abstract
{
    public interface ITableSaveState
    {
        IEnumerable<ElementStack> TableStacks { get; }
        List<Situation> Situations { get; }
        bool IsTableActive();
        MetaInfo MetaInfo { get; }
    }
}
