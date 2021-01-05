using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Interfaces
{

        public interface ILogMessage
        {
            string Description { get; set; }
            int MessageLevel { get; }
            int VerbosityNeeded { get; }
            Exception LoggedException { get; set; }
            string ToString();
        }
    
}
