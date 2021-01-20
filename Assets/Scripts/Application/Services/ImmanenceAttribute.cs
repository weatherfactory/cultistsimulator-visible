using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Application.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImmanenceAttribute: System.Attribute
    {
        public Type FallbackType { get; set; }
    }
}
