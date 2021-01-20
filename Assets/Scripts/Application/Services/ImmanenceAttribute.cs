using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImmanenceAttribute: System.Attribute
    {
        public Type FallbackType { get; set; }
    }
}
