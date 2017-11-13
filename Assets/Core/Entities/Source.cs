using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Enums;

namespace Assets.Core.Entities
{
    public class Source
    {
        public SourceType SourceType { get; set; }

        public static Source Existing()
        {
            var existingSource = new Source {SourceType = SourceType.Existing};
            return existingSource;
        }
    }
}
