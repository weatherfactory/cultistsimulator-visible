using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Enums;

namespace Assets.Core.Entities
{
    public class Source
    {
//What's all this, Alexis?
//I'm glad you asked, Alexis. It started out as convenience methods to return objects with enums,
//but then identity comparisons failed in unit tests so I stored static objects
//If I need to identify sources as more than SourceTypes, this may cause issues later
///I'm.... not sure if I need those actual method wrappers. But I like the syntax.
        private static readonly Source ExistingSource = new Source { SourceType = SourceType.Existing };
        private static readonly Source FreshSource = new Source { SourceType = SourceType.Fresh };

// What's all this, Alexis?
// I'm glad you asked, Alexis. The convenience method above returns a reference to these static instances
// And while the instances can not be changed (since readonly), their property "SourceType" could. So to
// avoid that, the SourceType can now only be set privately to avoid anyone changing it.
// Instead of changing sourcetype, just get a new Source reference directly.
// - Martin

        public SourceType SourceType { get; private set; }

        public static Source Existing()
        {
            return ExistingSource;
        }

        public static Source Fresh()
        {
            return FreshSource;
        }
    }
}
